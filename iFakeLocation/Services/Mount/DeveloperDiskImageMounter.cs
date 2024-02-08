using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using iMobileDevice;
using iMobileDevice.Afc;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.MobileImageMounter;
using iMobileDevice.Plist;

namespace iFakeLocation.Services.Mount
{
    internal class DeveloperDiskImageMounter : MobileImageMounter
    {

        public DeveloperDiskImageMounter(DeviceInformation device) : base(device) {
        }

        private enum DiskImageUploadMode {
            AFC,
            UploadImage
        }

        private static readonly MobileImageMounterUploadCallBack MounterUploadCallback = MounterReadCallback;

        private static int MounterReadCallback(IntPtr buffer, uint size, IntPtr userData) {
            var imageStream = (FileStream) GCHandle.FromIntPtr(userData).Target;
            var buf = new byte[size];
            var rl = imageStream.Read(buf, 0, buf.Length);
            Marshal.Copy(buf, 0, buffer, buf.Length);
            return rl;
        }

        public override void EnableDeveloperMode(string[] resourcePaths) {
            EnableDeveloperMode(resourcePaths[0], resourcePaths[1]);
        }

        private void EnableDeveloperMode(string deviceImagePath, string deviceImageSignaturePath) {
            if (!File.Exists(deviceImagePath) || !File.Exists(deviceImageSignaturePath))
                throw new FileNotFoundException("The specified device image files do not exist.");

            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownHandle = null;
            LockdownServiceDescriptorHandle serviceDescriptor = null;
            MobileImageMounterClientHandle mounterHandle = null;
            AfcClientHandle afcHandle = null;
            PlistHandle plistHandle = null;
            FileStream imageStream = null;

            // Use upload image for iOS 7 and above, otherwise use AFC
            DiskImageUploadMode mode = int.Parse(((string) _device.Properties["ProductVersion"]).Split('.')[0]) >= 7
                ? DiskImageUploadMode.UploadImage
                : DiskImageUploadMode.AFC;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var mounter = LibiMobileDevice.Instance.MobileImageMounter;
            var afc = LibiMobileDevice.Instance.Afc;

            try {
                // Get device handle
                if (idevice.idevice_new_with_options(out deviceHandle, _device.UDID, (int) (_device.IsNetwork ? iDeviceOptions.LookupNetwork : iDeviceOptions.LookupUsbmux)) != iDeviceError.Success)
                    throw new Exception("Unable to open device, is it connected?");

                // Get lockdownd handle
                if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "iFakeLocation") !=
                    LockdownError.Success)
                    throw new Exception("Unable to connect to lockdownd.");

                // Start image mounter service
                if (lockdown.lockdownd_start_service(lockdownHandle, "com.apple.mobile.mobile_image_mounter",
                        out serviceDescriptor) != LockdownError.Success)
                    throw new Exception("Unable to start the mobile image mounter service.");

                // Create mounter instance
                if (mounter.mobile_image_mounter_new(deviceHandle, serviceDescriptor, out mounterHandle) !=
                    MobileImageMounterError.Success)
                    throw new Exception("Unable to create mobile image mounter instance.");

                // Close service descriptor
                serviceDescriptor.Close();
                serviceDescriptor = null;

                // Start the AFC service
                if (mode == DiskImageUploadMode.AFC) {
                    if (lockdown.lockdownd_start_service(lockdownHandle, "com.apple.afc", out serviceDescriptor) !=
                        LockdownError.Success)
                        throw new Exception("Unable to start AFC service.");

                    if (afc.afc_client_new(deviceHandle, serviceDescriptor, out afcHandle) != AfcError.Success)
                        throw new Exception("Unable to connect to AFC service.");

                    serviceDescriptor.Close();
                    serviceDescriptor = null;
                }

                // Close lockdown handle
                lockdownHandle.Close();
                lockdownHandle = null;

                // Check if the developer image has already been mounted
                const string imageType = "Developer";
                if (mounter.mobile_image_mounter_lookup_image(mounterHandle, imageType, out plistHandle) ==
                    MobileImageMounterError.Success) {
                    var results =
                        PlistHelper.ReadPlistDictFromNode(plistHandle, new[] {"ImagePresent", "ImageSignature"});

                    // Some iOS use ImagePresent to verify presence, while others use ImageSignature instead
                    // Ensure to check the content of the ImageSignature value as iOS 14 returns a value even
                    // if it is empty.
                    if ((results.ContainsKey("ImagePresent") &&
                         results["ImagePresent"] is bool &&
                         (bool) results["ImagePresent"]) ||
                        (results.ContainsKey("ImageSignature") &&
                         results["ImageSignature"] is string &&
                         ((string)results["ImageSignature"]).IndexOf("<data>", StringComparison.InvariantCulture) >= 0))
                        return;
                }

                plistHandle.Close();
                plistHandle = null;

                // Configure paths for upload
                const string PkgPath = "PublicStaging";
                const string PathPrefix = "/private/var/mobile/Media";

                var targetName = PkgPath + "/staging.dimage";
                var mountName = PathPrefix + "/" + targetName;

                imageStream = new FileStream(deviceImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var sig = File.ReadAllBytes(deviceImageSignaturePath);

                switch (mode) {
                    case DiskImageUploadMode.UploadImage:
                        // Create stream for device image and wrap as a pointer for callback
                        var handle = GCHandle.Alloc(imageStream);
                        // Upload the image and then free unmanaged wrapper
                        mounter.mobile_image_mounter_upload_image(mounterHandle, imageType, (uint) imageStream.Length,
                            sig, (ushort) sig.Length, MounterUploadCallback, GCHandle.ToIntPtr(handle));
                        handle.Free();
                        break;
                    case DiskImageUploadMode.AFC:
                        // Create directory for package
                        ReadOnlyCollection<string> strs;
                        if (afc.afc_get_file_info(afcHandle, PkgPath, out strs) != AfcError.Success ||
                            afc.afc_make_directory(afcHandle, PkgPath) != AfcError.Success)
                            throw new Exception("Unable to create directory '" + PkgPath + "' on the device.");

                        // Create the target file
                        ulong af = 0;
                        if (afc.afc_file_open(afcHandle, targetName, AfcFileMode.FopenWronly, ref af) !=
                            AfcError.Success)
                            throw new Exception("Unable to create file '" + targetName + "'.");

                        // Read the file in chunks and write via AFC
                        uint amount = 0;
                        byte[] buf = new byte[8192];
                        do {
                            amount = (uint) imageStream.Read(buf, 0, buf.Length);
                            if (amount > 0) {
                                uint written = 0, total = 0;
                                while (total < amount) {
                                    // Write and ensure that it succeeded
                                    if (afc.afc_file_write(afcHandle, af, buf, amount, ref written) !=
                                        AfcError.Success) {
                                        afc.afc_file_close(afcHandle, af);
                                        throw new Exception("An AFC write error occurred.");
                                    }

                                    total += written;
                                }

                                if (total != amount) {
                                    afc.afc_file_close(afcHandle, af);
                                    throw new Exception("The developer image was not written completely.");
                                }
                            }
                        } while (amount > 0);

                        afc.afc_file_close(afcHandle, af);
                        break;
                }

                // Mount the image
                if (mounter.mobile_image_mounter_mount_image(mounterHandle, mountName, sig, (ushort) sig.Length,
                        imageType, out plistHandle) != MobileImageMounterError.Success)
                    throw new Exception("Unable to mount developer image.");

                // Parse the plist result
                var result = PlistHelper.ReadPlistDictFromNode(plistHandle);
                if (!result.ContainsKey("Status") ||
                    result["Status"] as string != "Complete")
                    throw new Exception("Mount failed with status: " +
                                        (result.ContainsKey("Status") ? result["Status"] : "N/A") + " and error: " +
                                        (result.ContainsKey("Error") ? result["Error"] : "N/A"));
            }
            finally {
                if (imageStream != null)
                    imageStream.Close();

                if (plistHandle != null)
                    plistHandle.Close();

                if (afcHandle != null)
                    afcHandle.Close();

                if (mounterHandle != null)
                    mounterHandle.Close();

                if (serviceDescriptor != null)
                    serviceDescriptor.Close();

                if (lockdownHandle != null)
                    lockdownHandle.Close();

                if (deviceHandle != null)
                    deviceHandle.Close();
            }
        }
    }
}
