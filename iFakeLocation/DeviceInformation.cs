using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using iMobileDevice;
using iMobileDevice.Afc;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.MobileImageMounter;
using iMobileDevice.Plist;
using iMobileDevice.PropertyListService;
using iMobileDevice.Service;

namespace iFakeLocation {
    class DeviceInformation {
        private static readonly Dictionary<string, string> RealProductName = new Dictionary<string, string> {
            {"i386", "iPhone Simulator"},
            {"x86_64", "iPhone Simulator"},
            {"arm64", "iPhone Simulator"},
            {"iPhone1,1", "iPhone"},
            {"iPhone1,2", "iPhone 3G"},
            {"iPhone2,1", "iPhone 3GS"},
            {"iPhone3,1", "iPhone 4"},
            {"iPhone3,2", "iPhone 4 GSM Rev A"},
            {"iPhone3,3", "iPhone 4 CDMA"},
            {"iPhone4,1", "iPhone 4S"},
            {"iPhone5,1", "iPhone 5 (GSM)"},
            {"iPhone5,2", "iPhone 5 (GSM+CDMA)"},
            {"iPhone5,3", "iPhone 5C (GSM)"},
            {"iPhone5,4", "iPhone 5C (Global)"},
            {"iPhone6,1", "iPhone 5S (GSM)"},
            {"iPhone6,2", "iPhone 5S (Global)"},
            {"iPhone7,1", "iPhone 6 Plus"},
            {"iPhone7,2", "iPhone 6"},
            {"iPhone8,1", "iPhone 6s"},
            {"iPhone8,2", "iPhone 6s Plus"},
            {"iPhone8,4", "iPhone SE (GSM)"},
            {"iPhone9,1", "iPhone 7"},
            {"iPhone9,2", "iPhone 7 Plus"},
            {"iPhone9,3", "iPhone 7"},
            {"iPhone9,4", "iPhone 7 Plus"},
            {"iPhone10,1", "iPhone 8"},
            {"iPhone10,2", "iPhone 8 Plus"},
            {"iPhone10,3", "iPhone X Global"},
            {"iPhone10,4", "iPhone 8"},
            {"iPhone10,5", "iPhone 8 Plus"},
            {"iPhone10,6", "iPhone X GSM"},
            {"iPhone11,2", "iPhone XS"},
            {"iPhone11,4", "iPhone XS Max"},
            {"iPhone11,6", "iPhone XS Max Global"},
            {"iPhone11,8", "iPhone XR"},
            {"iPhone12,1", "iPhone 11"},
            {"iPhone12,3", "iPhone 11 Pro"},
            {"iPhone12,5", "iPhone 11 Pro Max"},
            {"iPhone12,8", "iPhone SE 2nd Gen"},
            {"iPhone13,1", "iPhone 12 Mini"},
            {"iPhone13,2", "iPhone 12"},
            {"iPhone13,3", "iPhone 12 Pro"},
            {"iPhone13,4", "iPhone 12 Pro Max"},
            {"iPhone14,2", "iPhone 13 Pro"},
            {"iPhone14,3", "iPhone 13 Pro Max"},
            {"iPhone14,4", "iPhone 13 Mini"},
            {"iPhone14,5", "iPhone 13"},
            {"iPhone14,6", "iPhone SE 3rd Gen"},

            {"iPod1,1", "1st Gen iPod"},
            {"iPod2,1", "2nd Gen iPod"},
            {"iPod3,1", "3rd Gen iPod"},
            {"iPod4,1", "4th Gen iPod"},
            {"iPod5,1", "5th Gen iPod"},
            {"iPod7,1", "6th Gen iPod"},
            {"iPod9,1", "7th Gen iPod"},

            {"iPad1,1", "iPad"},
            {"iPad1,2", "iPad 3G"},
            {"iPad2,1", "2nd Gen iPad"},
            {"iPad2,2", "2nd Gen iPad GSM"},
            {"iPad2,3", "2nd Gen iPad CDMA"},
            {"iPad2,4", "2nd Gen iPad New Revision"},
            {"iPad3,1", "3rd Gen iPad"},
            {"iPad3,2", "3rd Gen iPad CDMA"},
            {"iPad3,3", "3rd Gen iPad GSM"},
            {"iPad2,5", "iPad mini"},
            {"iPad2,6", "iPad mini GSM+LTE"},
            {"iPad2,7", "iPad mini CDMA+LTE"},
            {"iPad3,4", "4th Gen iPad"},
            {"iPad3,5", "4th Gen iPad GSM+LTE"},
            {"iPad3,6", "4th Gen iPad CDMA+LTE"},
            {"iPad4,1", "iPad Air (WiFi)"},
            {"iPad4,2", "iPad Air (GSM+CDMA)"},
            {"iPad4,3", "1st Gen iPad Air (China)"},
            {"iPad4,4", "iPad mini Retina (WiFi)"},
            {"iPad4,5", "iPad mini Retina (GSM+CDMA)"},
            {"iPad4,6", "iPad mini Retina (China)"},
            {"iPad4,7", "iPad mini 3 (WiFi)"},
            {"iPad4,8", "iPad mini 3 (GSM+CDMA)"},
            {"iPad4,9", "iPad Mini 3 (China)"},
            {"iPad5,1", "iPad mini 4 (WiFi)"},
            {"iPad5,2", "4th Gen iPad mini (WiFi+Cellular)"},
            {"iPad5,3", "iPad Air 2 (WiFi)"},
            {"iPad5,4", "iPad Air 2 (Cellular)"},
            {"iPad6,3", "iPad Pro (9.7 inch, WiFi)"},
            {"iPad6,4", "iPad Pro (9.7 inch, WiFi+LTE)"},
            {"iPad6,7", "iPad Pro (12.9 inch, WiFi)"},
            {"iPad6,8", "iPad Pro (12.9 inch, WiFi+LTE)"},
            {"iPad6,11", "iPad (2017)"},
            {"iPad6,12", "iPad (2017)"},
            {"iPad7,1", "iPad Pro 2nd Gen (WiFi)"},
            {"iPad7,2", "iPad Pro 2nd Gen (WiFi+Cellular)"},
            {"iPad7,3", "iPad Pro 10.5-inch 2nd Gen"},
            {"iPad7,4", "iPad Pro 10.5-inch 2nd Gen"},
            {"iPad7,5", "iPad 6th Gen (WiFi)"},
            {"iPad7,6", "iPad 6th Gen (WiFi+Cellular)"},
            {"iPad7,11", "iPad 7th Gen 10.2-inch (WiFi)"},
            {"iPad7,12", "iPad 7th Gen 10.2-inch (WiFi+Cellular)"},
            {"iPad8,1", "iPad Pro 11 inch 3rd Gen (WiFi)"},
            {"iPad8,2", "iPad Pro 11 inch 3rd Gen (1TB, WiFi)"},
            {"iPad8,3", "iPad Pro 11 inch 3rd Gen (WiFi+Cellular)"},
            {"iPad8,4", "iPad Pro 11 inch 3rd Gen (1TB, WiFi+Cellular)"},
            {"iPad8,5", "iPad Pro 12.9 inch 3rd Gen (WiFi)"},
            {"iPad8,6", "iPad Pro 12.9 inch 3rd Gen (1TB, WiFi)"},
            {"iPad8,7", "iPad Pro 12.9 inch 3rd Gen (WiFi+Cellular)"},
            {"iPad8,8", "iPad Pro 12.9 inch 3rd Gen (1TB, WiFi+Cellular)"},
            {"iPad8,9", "iPad Pro 11 inch 4th Gen (WiFi)"},
            {"iPad8,10", "iPad Pro 11 inch 4th Gen (WiFi+Cellular)"},
            {"iPad8,11", "iPad Pro 12.9 inch 4th Gen (WiFi)"},
            {"iPad8,12", "iPad Pro 12.9 inch 4th Gen (WiFi+Cellular)"},
            {"iPad11,1", "iPad mini 5th Gen (WiFi)"},
            {"iPad11,2", "iPad mini 5th Gen"},
            {"iPad11,3", "iPad Air 3rd Gen (WiFi)"},
            {"iPad11,4", "iPad Air 3rd Gen"},
            {"iPad11,6", "iPad 8th Gen (WiFi)"},
            {"iPad11,7", "iPad 8th Gen (WiFi+Cellular)"},
            {"iPad12,1", "iPad 9th Gen (WiFi)"},
            {"iPad12,2", "iPad 9th Gen (WiFi+Cellular)"},
            {"iPad14,1", "iPad mini 6th Gen (WiFi)"},
            {"iPad14,2", "iPad mini 6th Gen (WiFi+Cellular)"},
            {"iPad13,1", "iPad Air 4th Gen (WiFi)"},
            {"iPad13,2", "iPad Air 4th Gen (WiFi+Cellular)"},
            {"iPad13,4", "iPad Pro 11 inch 5th Gen"},
            {"iPad13,5", "iPad Pro 11 inch 5th Gen"},
            {"iPad13,6", "iPad Pro 11 inch 5th Gen"},
            {"iPad13,7", "iPad Pro 11 inch 5th Gen"},
            {"iPad13,8", "iPad Pro 12.9 inch 5th Gen"},
            {"iPad13,9", "iPad Pro 12.9 inch 5th Gen"},
            {"iPad13,10", "iPad Pro 12.9 inch 5th Gen"},
            {"iPad13,11", "iPad Pro 12.9 inch 5th Gen"},
            {"iPad13,16", "iPad Air 5th Gen (WiFi)"},
            {"iPad13,17", "iPad Air 5th Gen (WiFi+Cellular)"}
        };

        public string Name { get; }
        public string UDID { get; }
        public Dictionary<string, object> Properties { get; private set; }

        private DeviceInformation(string name, string udid) {
            Name = name;
            UDID = udid;
            Properties = new Dictionary<string, object>();
        }

        private void ReadProperties(PlistHandle node) {
            Properties =
                PlistReader.ReadPlistDictFromNode(node, new[] {"ProductType", "ProductVersion", "HostAttached"});
        }

        public override string ToString() {
            var sb = new StringBuilder().Append(Name).Append(" (");

            if (Properties.ContainsKey("ProductType")) {
                if (Properties["ProductType"] is string && RealProductName.ContainsKey((string) Properties["ProductType"]))
                    sb.Append(RealProductName[(string) Properties["ProductType"]]);
                else
                    sb.Append(Properties["ProductType"]);
            }

            if (Properties.ContainsKey("ProductVersion"))
                sb.Append("; iOS ").Append(Properties["ProductVersion"]);

            sb.Append(')');
            return sb.ToString();
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

        public enum DeveloperModeToggleState {
            NA,
            Visible,
            Hidden
        }

        public DeveloperModeToggleState GetDeveloperModeToggleState() {
            // Toggle only exists on iOS 16 onwards
            if (int.Parse(((string)Properties["ProductVersion"]).Split('.')[0]) < 16) {
                return DeveloperModeToggleState.NA;
            }

            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownHandle = null;
            PlistHandle plistHandle = null;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var plist = LibiMobileDevice.Instance.Plist;

            try {
                // Get device handle
                if (idevice.idevice_new(out deviceHandle, UDID) != iDeviceError.Success)
                    throw new Exception("Unable to open device, is it connected?");

                // Get lockdownd handle
                if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "iFakeLocation") !=
                    LockdownError.Success) {
                    throw new Exception("Unable to connect to lockdownd.");
                }

                // Check AMFI DeveloperModeStatus property
                if (lockdown.lockdownd_get_value(lockdownHandle, "com.apple.security.mac.amfi", "DeveloperModeStatus",
                        out plistHandle) !=
                    LockdownError.Success) {
                    throw new Exception("Unable to query com.apple.security.mac.amfi service.");
                }

                char status = '\0';
                plist.plist_get_bool_val(plistHandle, ref status);
                return status > 0 ? DeveloperModeToggleState.Visible : DeveloperModeToggleState.Hidden;
            }
            finally {
                if (plistHandle != null)
                    plistHandle.Close();

                if (lockdownHandle != null)
                    lockdownHandle.Close();

                if (deviceHandle != null)
                    deviceHandle.Close();
            }
        }

        public void EnableDeveloperModeToggle() {
            // Toggle only exists on iOS 16 onwards
            if (int.Parse(((string)Properties["ProductVersion"]).Split('.')[0]) < 16) {
                return;
            }

            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownHandle = null;
            LockdownServiceDescriptorHandle serviceDescriptor = null;
            PropertyListServiceClientHandle propertyListServiceClientHandle = null;
            PlistHandle plistHandle = null;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var plist = LibiMobileDevice.Instance.Plist;
            var propertyListService = LibiMobileDevice.Instance.PropertyListService;

            try {
                // Get device handle
                if (idevice.idevice_new(out deviceHandle, UDID) != iDeviceError.Success)
                    throw new Exception("Unable to open device, is it connected?");

                // Get lockdownd handle
                if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "iFakeLocation") !=
                    LockdownError.Success) {
                    throw new Exception("Unable to connect to lockdownd.");
                }

                // Start the com.apple.amfi.lockdown service
                if (lockdown.lockdownd_start_service(lockdownHandle, "com.apple.amfi.lockdown", out serviceDescriptor) !=
                    LockdownError.Success) {
                    throw new Exception("Unable to start the com.apple.amfi.lockdown service.");
                }

                // Create property list service client.
                if (propertyListService.property_list_service_client_new(deviceHandle, serviceDescriptor,
                        out propertyListServiceClientHandle) != PropertyListServiceError.Success) {
                    throw new Exception("Unable to create property list service client.");
                }

                // Create and send plist to the AMFI lockdown server
                plistHandle = plist.plist_new_dict();

                // 0 = reveal toggle in settings
                // 1 = enable developer mode (only if no passcode is set)
                // 2 = answers developer mode enable prompt post-restart?
                plist.plist_dict_set_item(plistHandle, "action", plist.plist_new_uint(0));

                if (propertyListService.property_list_service_send_xml_plist(propertyListServiceClientHandle,
                        plistHandle) != PropertyListServiceError.Success) {
                    throw new Exception("Failed to send request to enable developer mode toggle.");
                }
                plistHandle.Close();
                plistHandle = null;

                // Parse the response from the service
                if (propertyListService.property_list_service_receive_plist(propertyListServiceClientHandle,
                        out plistHandle) != PropertyListServiceError.Success) {
                    throw new Exception("Failed to retrieve response after attempting to enable developer mode toggle.");
                }

                var dict = PlistReader.ReadPlistDictFromNode(plistHandle);
                if (dict.ContainsKey("Error")) {
                    throw new Exception("Failed to enable the developer mode toggle: " + dict["Error"]);
                } else if (dict.ContainsKey("success")) {
                    if (!(bool)dict["success"]) {
                        throw new Exception("Failed to enable the developer mode toggle (unknown error)");
                    }
                }
                else {
                    throw new Exception("Failed to enable the developer mode toggle (unexpected response)");
                }
            }
            finally {
                if (plistHandle != null)
                    plistHandle.Close();

                if (propertyListServiceClientHandle != null)
                    propertyListServiceClientHandle.Close();

                if (serviceDescriptor != null)
                    serviceDescriptor.Close();

                if (lockdownHandle != null)
                    lockdownHandle.Close();

                if (deviceHandle != null)
                    deviceHandle.Close();
            }
        }

        public void EnableDeveloperMode(string deviceImagePath, string deviceImageSignaturePath) {
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
            DiskImageUploadMode mode = int.Parse(((string) Properties["ProductVersion"]).Split('.')[0]) >= 7
                ? DiskImageUploadMode.UploadImage
                : DiskImageUploadMode.AFC;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var mounter = LibiMobileDevice.Instance.MobileImageMounter;
            var afc = LibiMobileDevice.Instance.Afc;

            try {
                // Get device handle
                if (idevice.idevice_new(out deviceHandle, UDID) != iDeviceError.Success)
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
                        PlistReader.ReadPlistDictFromNode(plistHandle, new[] {"ImagePresent", "ImageSignature"});

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
                var result = PlistReader.ReadPlistDictFromNode(plistHandle);
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

        private static byte[] ToBytesBE(int i) {
            var b = BitConverter.GetBytes((uint) i);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        public void SetLocation(PointLatLng target) {
            SetLocation(this, target);
        }

        public void StopLocation() {
            SetLocation(this, null);
        }

        private static void SetLocation(DeviceInformation deviceInfo, PointLatLng? target) {
            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownHandle = null;
            LockdownServiceDescriptorHandle simulateDescriptor = null;
            ServiceClientHandle serviceClientHandle = null;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var service = LibiMobileDevice.Instance.Service;

            try {
                // Get device handle
                var err = idevice.idevice_new(out deviceHandle, deviceInfo.UDID);
                if (err != iDeviceError.Success)
                    throw new Exception("Unable to connect to the device. Make sure it is connected.");

                // Obtain a lockdown client handle
                if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "iFakeLocation") !=
                    LockdownError.Success)
                    throw new Exception("Unable to connect to lockdownd.");

                // Start the simulatelocation service
                if (lockdown.lockdownd_start_service(lockdownHandle, "com.apple.dt.simulatelocation",
                        out simulateDescriptor) != LockdownError.Success ||
                    simulateDescriptor.IsInvalid)
                    throw new Exception("Unable to start simulatelocation service.");

                // Create new service client
                if (service.service_client_new(deviceHandle, simulateDescriptor, out serviceClientHandle) !=
                    ServiceError.Success)
                    throw new Exception("Unable to create simulatelocation service client.");

                if (!target.HasValue) {
                    // Send stop
                    var stopMessage = ToBytesBE(1); // 0x1 (32-bit big-endian uint)
                    uint sent = 0;
                    if (service.service_send(serviceClientHandle, stopMessage, (uint) stopMessage.Length, ref sent) !=
                        ServiceError.Success)
                        throw new Exception("Unable to send stop message to device.");
                }
                else {
                    // Send start
                    var startMessage = ToBytesBE(0); // 0x0 (32-bit big-endian uint)
                    var lat = Encoding.ASCII.GetBytes(target.Value.Lat.ToString(CultureInfo.InvariantCulture));
                    var lng = Encoding.ASCII.GetBytes(target.Value.Lng.ToString(CultureInfo.InvariantCulture));
                    var latLen = ToBytesBE(lat.Length);
                    var lngLen = ToBytesBE(lng.Length);
                    uint sent = 0;

                    if (service.service_send(serviceClientHandle, startMessage, (uint) startMessage.Length, ref sent) !=
                        ServiceError.Success ||
                        service.service_send(serviceClientHandle, latLen, (uint) latLen.Length, ref sent) !=
                        ServiceError.Success ||
                        service.service_send(serviceClientHandle, lat, (uint) lat.Length, ref sent) !=
                        ServiceError.Success ||
                        service.service_send(serviceClientHandle, lngLen, (uint) lngLen.Length, ref sent) !=
                        ServiceError.Success ||
                        service.service_send(serviceClientHandle, lng, (uint) lng.Length, ref sent) !=
                        ServiceError.Success) {
                        throw new Exception("Unable to send co-ordinates to device.");
                    }
                }
            }
            finally {
                // Cleanup
                if (serviceClientHandle != null)
                    serviceClientHandle.Close();

                if (simulateDescriptor != null)
                    simulateDescriptor.Close();

                if (lockdownHandle != null)
                    lockdownHandle.Close();

                if (deviceHandle != null)
                    deviceHandle.Close();
            }
        }

        public static List<DeviceInformation> GetDevices() {
            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var plist = LibiMobileDevice.Instance.Plist;

            // Retrieve list of unique device identifiers
            ReadOnlyCollection<string> uddids;
            int count = 0;
            var ret = idevice.idevice_get_device_list(out uddids, ref count);
            if (ret != iDeviceError.Success)
                return null;

            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownHandle = null;
            PlistHandle plistHandle = null;

            var devices = new List<DeviceInformation>();
            foreach (var udid in uddids.Distinct()) {
                try {
                    // Attempt to get device handle of each uuid
                    var err = idevice.idevice_new(out deviceHandle, udid);
                    if (err != iDeviceError.Success)
                        continue;

                    // Obtain a lockdown client handle
                    if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle,
                            "iFakeLocation") !=
                        LockdownError.Success)
                        continue;

                    // Obtain the device name
                    string name;
                    DeviceInformation device;
                    if (lockdown.lockdownd_get_device_name(lockdownHandle, out name) != LockdownError.Success)
                        continue;

                    device = new DeviceInformation(name, udid);

                    // Get device details
                    if (lockdown.lockdownd_get_value(lockdownHandle, null, null, out plistHandle) !=
                        LockdownError.Success ||
                        plist.plist_get_node_type(plistHandle) != PlistType.Dict)
                        continue;

                    device.ReadProperties(plistHandle);

                    // Ensure device is attached
                    if (!device.Properties.ContainsKey("HostAttached") || (bool) device.Properties["HostAttached"])
                        devices.Add(device);
                }
                finally {
                    // Cleanup
                    if (plistHandle != null)
                        plistHandle.Close();
                    if (lockdownHandle != null)
                        lockdownHandle.Close();
                    if (deviceHandle != null)
                        deviceHandle.Close();
                }
            }

            return devices;
        }
    }
}