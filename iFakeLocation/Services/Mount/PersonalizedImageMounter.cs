using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using iFakeLocation.Services.Restore;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Plist;
using iMobileDevice.PropertyListService;
using iMobileDevice.Service;

namespace iFakeLocation.Services.Mount
{
    internal class PersonalizedImageMounter : MobileImageMounter {
        public PersonalizedImageMounter(DeviceInformation device) : base(device) {
        }

        public override void EnableDeveloperMode(string[] resourcePaths) {
            EnableDeveloperMode(resourcePaths[0], resourcePaths[1], resourcePaths[2]);
        }

        private static Dictionary<string, object> SendRecvPlist(PropertyListServiceClientHandle propListServiceHandle,
            PlistHandle plist,
            bool isXml = true) {
            var propListService = LibiMobileDevice.Instance.PropertyListService;

            PlistHandle plistOutHandle = null;

            try {
                if ((isXml ? propListService.property_list_service_send_xml_plist(propListServiceHandle, plist) : 
                        propListService.property_list_service_send_binary_plist(propListServiceHandle, plist)) !=
                    PropertyListServiceError.Success) 
                    throw new Exception("Failed to send the plist to the specified service.");

                if (propListService.property_list_service_receive_plist(propListServiceHandle,
                        out plistOutHandle) != PropertyListServiceError.Success)
                    throw new Exception("Failed to receive the plist from the specified service.");

                return PlistHelper.ReadPlistDictFromNode(plistOutHandle);
            }
            finally {
                if (plistOutHandle != null)
                    plistOutHandle.Close();
            }
        }

        private static Dictionary<string, object> SendDataRecvPlist(PropertyListServiceClientHandle propListServiceHandle, byte[] data) {
            var propListService = LibiMobileDevice.Instance.PropertyListService;
            var service = LibiMobileDevice.Instance.Service;

            PlistHandle plistOutHandle = null;
            ServiceClientHandle serviceClientHandle = null;

            try {
                // Extract the service client from the property_list_service_t->parent value (warning: hacky)
                // struct property_list_service_client_private {
                //      service_client_t parent;
                // };
                serviceClientHandle = ServiceClientHandle.DangerousCreate(Marshal.ReadIntPtr(propListServiceHandle.DangerousGetHandle()));

                uint sent = 0;
                if (service.service_send(serviceClientHandle, data, (uint)data.Length, ref sent) !=
                    ServiceError.Success ||
                    sent != data.Length) {
                    throw new Exception("Failed to send the data to the specified service.");
                }

                if (propListService.property_list_service_receive_plist(propListServiceHandle,
                        out plistOutHandle) != PropertyListServiceError.Success)
                    throw new Exception("Failed to receive the plist from the specified service.");

                return PlistHelper.ReadPlistDictFromNode(plistOutHandle);
            }
            finally {
                // Ensure CLR does not attempt to close the handle during destruction of the SafeFileHandle
                // since we extracted this handle manually (we will get an exception otherwise during garbage collection)
                if (serviceClientHandle != null)
                    serviceClientHandle.SetHandleAsInvalid();

                if (plistOutHandle != null)
                    plistOutHandle.Close();
            }
        }

        private Dictionary<string, object> QueryPersonalizationIdentifiers(PropertyListServiceClientHandle propListServiceHandle) {
            var plist = LibiMobileDevice.Instance.Plist;

            var plistHandle = plist.plist_new_dict();

            try {
                plist.plist_dict_set_item(plistHandle, "Command",
                    plist.plist_new_string("QueryPersonalizationIdentifiers"));

                return SendRecvPlist(propListServiceHandle, plistHandle);
            }
            finally {
                if (plistHandle != null)
                    plistHandle.Close();
            }
        }

        private byte[] QueryNonce(PropertyListServiceClientHandle propListServiceHandle,
            string personalizedImageType = null) {
            var plist = LibiMobileDevice.Instance.Plist;

            var plistHandle = plist.plist_new_dict();

            try {
                plist.plist_dict_set_item(plistHandle, "Command",
                    plist.plist_new_string("QueryNonce"));
                if (personalizedImageType != null) {
                    plist.plist_dict_set_item(plistHandle, "PersonalizedImageType",
                        plist.plist_new_string(personalizedImageType));
                }

                var result = SendRecvPlist(propListServiceHandle, plistHandle);
                if (!result.ContainsKey("PersonalizationNonce"))
                    throw new Exception("Unable to locate personalization nonce in response.");

                return (byte[])result["PersonalizationNonce"];
            }
            finally {
                if (plistHandle != null)
                    plistHandle.Close();
            }
        }

        private byte[] GetManifestFromTSS(PropertyListServiceClientHandle propListServiceHandle, Dictionary<string, object> buildManifest) {
            // Obtain the personalization identifiers from the device
            var identifiers = QueryPersonalizationIdentifiers(propListServiceHandle);
            if (identifiers == null || !identifiers.ContainsKey("PersonalizationIdentifiers"))
                throw new Exception("Failed to extract personalization identifiers from the plist response.");
            var personalizationIdentifiers = (Dictionary<string, object>)identifiers["PersonalizationIdentifiers"];

            var request = new TSSRequest();

            // Pass through any important identifiers to the TSS request
            foreach (var kvp in personalizationIdentifiers) {
                if (kvp.Key.StartsWith("Ap,"))
                    request.Update(kvp.Key, kvp.Value);
            }

            // Find a matching build identity from the BuildManifest.plist file
            var boardId = int.Parse(personalizationIdentifiers["BoardId"].ToString());
            var chipId = int.Parse(personalizationIdentifiers["ChipID"].ToString());
            Dictionary<string, object> buildIdentity = null;
            foreach (Dictionary<string, object> identity in (object[])buildManifest["BuildIdentities"]) {
                var curBoardId = identity.ContainsKey("ApBoardID") ? int.Parse(((string) identity["ApBoardID"]).Replace("0x", ""), NumberStyles.HexNumber) : 0;
                var curChipId = identity.ContainsKey("ApChipID") ? int.Parse(((string) identity["ApChipID"]).Replace("0x", ""), NumberStyles.HexNumber) : 0;
                if (curBoardId == boardId && curChipId == chipId) {
                    buildIdentity = identity;
                    break;
                }
            }

            if (buildIdentity == null)
                throw new Exception(
                    "Unable to find a build identity matching the current device in the build manifest.");

            request.Update(new Dictionary<string, object> {
                {"@ApImg4Ticket", true},
                {"@BBTicket", true},
                {"ApBoardID", boardId},
                {"ApChipID", chipId},
                {"ApECID", _device.Properties["UniqueChipID"]},
                {"ApNonce", QueryNonce(propListServiceHandle, "DeveloperDiskImage")},
                {"ApProductionMode", true},
                {"ApSecurityDomain", 1},
                {"ApSecurityMode", true},
                {"SepNonce", Encoding.ASCII.GetBytes("\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00")},
                {"UID_MODE", false}
            });

            var parameters = new Dictionary<string, object> {
                {"ApProductionMode", true},
                {"ApSecurityDomain", 1},
                {"ApSecurityMode", true},
                {"ApSupportsImg4", true}
            };

            var manifest = (Dictionary<string, object>)buildIdentity["Manifest"];
            foreach (var kvp in manifest) {
                var manifestEntry = (Dictionary<string, object>)kvp.Value;

                // Only permit trusted items
                if (!manifestEntry.ContainsKey("Info") || manifestEntry["Info"] == null)
                    continue;
                if (!manifestEntry.ContainsKey("Trusted") || !(bool) manifestEntry["Trusted"])
                    continue;

                var tssEntry = new Dictionary<string, object>(manifestEntry);
                tssEntry.Remove("Info");

                // Apply the restore request rules
                if (((Dictionary<string, object>)((Dictionary<string, object>)manifest["LoadableTrustCache"])["Info"])
                    .ContainsKey("RestoreRequestRules")) {
                    var rules =
                        (object[])
                        ((Dictionary<string, object>)((Dictionary<string, object>)manifest["LoadableTrustCache"])[
                            "Info"])["RestoreRequestRules"];
                    if (rules?.Length > 0) {
                        request.ApplyRestoreRequestRules(tssEntry, parameters, rules.Select(s => (Dictionary<string, object>) s));
                    }
                }

                // Ensure a digest always exists
                if (!manifestEntry.ContainsKey("Digest") || manifestEntry["Digest"] == null) {
                    tssEntry["Digest"] = Array.Empty<byte>();
                }

                request.Update(kvp.Key, tssEntry);
            }

            var tssResponse = request.SendAndReceive();
            if (!tssResponse.ContainsKey("ApImg4Ticket"))
                throw new Exception("TSS response did not contain the expected ticket.");

            return (byte[]) tssResponse["ApImg4Ticket"];
        }

        private byte[] QueryPersonalizationManifest(PropertyListServiceClientHandle propListServiceHandle, string imageType, byte[] signature) {
            var plist = LibiMobileDevice.Instance.Plist;

            var plistHandle = plist.plist_new_dict();

            try {
                plist.plist_dict_set_item(plistHandle, "Command",
                    plist.plist_new_string("QueryPersonalizationManifest"));
                plist.plist_dict_set_item(plistHandle, "PersonalizedImageType",
                        plist.plist_new_string(imageType));
                plist.plist_dict_set_item(plistHandle, "ImageType",
                    plist.plist_new_string(imageType));
                plist.plist_dict_set_item(plistHandle, "ImageSignature",
                    NativeMethods.plist_new_data(signature, signature.Length));

                var result = SendRecvPlist(propListServiceHandle, plistHandle);
                if (!result.ContainsKey("ImageSignature"))
                    throw new KeyNotFoundException("Unable to locate image signature in response.");

                return (byte[])result["ImageSignature"];
            }
            finally {
                if (plistHandle != null)
                    plistHandle.Close();
            }
        }

        private void UploadPersonalizedImage(PropertyListServiceClientHandle propListServiceHandle, string imageType,
            byte[] image, byte[] signature) {
            var plist = LibiMobileDevice.Instance.Plist;

            var plistHandle = plist.plist_new_dict();

            try {
                // Let the service know we are about to upload an image
                plist.plist_dict_set_item(plistHandle, "Command",
                    plist.plist_new_string("ReceiveBytes"));
                plist.plist_dict_set_item(plistHandle, "ImageType",
                    plist.plist_new_string(imageType));
                plist.plist_dict_set_item(plistHandle, "ImageSize",
                    plist.plist_new_uint((uint) image.Length));
                plist.plist_dict_set_item(plistHandle, "ImageSignature",
                    NativeMethods.plist_new_data(signature, signature.Length));

                var result = SendRecvPlist(propListServiceHandle, plistHandle);
                if (!result.ContainsKey("Status") || (string)result["Status"] != "ReceiveBytesAck")
                    throw new Exception("Failed to upload the image to the device: " + (result.ContainsKey("Error") ? result["Error"] : "Unknown error"));

                // Send the image and then read the plist response
                result = SendDataRecvPlist(propListServiceHandle, image);
                if (!result.ContainsKey("Status") || (string)result["Status"] != "Complete")
                    throw new Exception("Failed to validate that the image upload successfully.");
            }
            finally {
                if (plistHandle != null)
                    plistHandle.Close();
            }
        }

        private void MountPersonalizedImage(PropertyListServiceClientHandle propListServiceHandle, string imageType, byte[] signature,
            Action<IPlistApi, PlistHandle> extraPropsAction = null) {
            var plist = LibiMobileDevice.Instance.Plist;

            var plistHandle = plist.plist_new_dict();

            try {
                // Instruct device to mount previously uploaded image
                plist.plist_dict_set_item(plistHandle, "Command",
                    plist.plist_new_string("MountImage"));
                plist.plist_dict_set_item(plistHandle, "ImageType",
                    plist.plist_new_string(imageType));
                plist.plist_dict_set_item(plistHandle, "ImageSignature",
                    NativeMethods.plist_new_data(signature, signature.Length));

                // Augment plist if required
                if (extraPropsAction != null) {
                    extraPropsAction(plist, plistHandle);
                }

                var result = SendRecvPlist(propListServiceHandle, plistHandle);
                
                if (result.ContainsKey("DetailedError") &&
                    ((string)result["DetailedError"]).Contains("Developer mode is not enabled")) {
                    throw new Exception("Developer mode is not enabled on the device.");
                }

                if (result.ContainsKey("DetailedError") &&
                    ((string)result["DetailedError"]).Contains("is already mounted"))
                    return;

                if (!result.ContainsKey("Status") || (string)result["Status"] != "Complete")
                    throw new Exception("Failed to mount the personalized image.");
            }
            finally {
                if (plistHandle != null)
                    plistHandle.Close();
            }
        }

        private static bool IsPersonalizedImageMounted(PropertyListServiceClientHandle propListServiceHandle, string imageType) {
            var plist = LibiMobileDevice.Instance.Plist;

            var plistHandle = plist.plist_new_dict();

            try {
                plist.plist_dict_set_item(plistHandle, "Command",
                    plist.plist_new_string("LookupImage"));
                plist.plist_dict_set_item(plistHandle, "ImageType",
                    plist.plist_new_string(imageType));

                var result = SendRecvPlist(propListServiceHandle, plistHandle);
                return (result.ContainsKey("ImagePresent") && (bool)result["ImagePresent"]) ||
                       (result.ContainsKey("ImageSignature") &&
                        ((result["ImageSignature"] is object[] && ((object[])result["ImageSignature"]).Length > 0) ||
                         (result["ImageSignature"] is not object[] && result["ImageSignature"] != null)));
            }
            finally {
                if (plistHandle != null)
                    plistHandle.Close();
            }
        }

        private void EnableDeveloperMode(string imagePath, string buildManifestPath, string trustCachePath, bool useExistingManifest = true) {
            if (!File.Exists(imagePath) || !File.Exists(buildManifestPath) || !File.Exists(trustCachePath))
                throw new FileNotFoundException("The specified device image files do not exist.");

            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownHandle = null;
            LockdownServiceDescriptorHandle serviceDescriptor = null;
            PropertyListServiceClientHandle propListServiceHandle = null;

            void CloseAllHandles() {
                if (propListServiceHandle != null)
                    propListServiceHandle.Close();

                if (serviceDescriptor != null)
                    serviceDescriptor.Close();

                if (lockdownHandle != null)
                    lockdownHandle.Close();

                if (deviceHandle != null)
                    deviceHandle.Close();

                propListServiceHandle = null;
                serviceDescriptor = null;
                lockdownHandle = null;
                deviceHandle = null;
            }

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var propListService = LibiMobileDevice.Instance.PropertyListService;

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

                // Create new plist service client
                if (propListService.property_list_service_client_new(deviceHandle, serviceDescriptor,
                        out propListServiceHandle) != PropertyListServiceError.Success)
                    throw new Exception("Failed to obtain a property list service handle.");

                // Sanity check to skip upload/mount
                if (IsPersonalizedImageMounted(propListServiceHandle, "Personalized"))
                    return;

                // Obtain the personalization manifest from device, otherwise request a new one
                byte[] manifest;

                if (useExistingManifest) {
                    try {
                        using var imageStream = File.OpenRead(imagePath);
                        manifest = QueryPersonalizationManifest(propListServiceHandle, "DeveloperDiskImage",
                            SHA384.Create().ComputeHash(imageStream));
                    }
                    catch (KeyNotFoundException) {
                        // We need to run this function again (without querying for manifest) as service connection will be dead now
                        CloseAllHandles();
                        EnableDeveloperMode(imagePath, buildManifestPath, trustCachePath, false);
                        return;
                    }
                }
                else {
                    using var manifestStream = File.OpenRead(buildManifestPath);
                    manifest = GetManifestFromTSS(propListServiceHandle,
                        PlistHelper.ReadPlistDictFromStream(manifestStream));
                }

                // Upload the image to the device
                UploadPersonalizedImage(propListServiceHandle, "Personalized", File.ReadAllBytes(imagePath), manifest);

                // Mount the image
                MountPersonalizedImage(propListServiceHandle, "Personalized", manifest, (plist, plistHandle) => {
                    var trustCache = File.ReadAllBytes(trustCachePath);
                    plist.plist_dict_set_item(plistHandle, "ImageTrustCache",
                        NativeMethods.plist_new_data(trustCache, trustCache.Length));
                });
            }
            finally {
                CloseAllHandles();
            }
        }
    }
}
