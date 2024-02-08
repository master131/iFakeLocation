using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using iFakeLocation.Services;
using iFakeLocation.Services.Location;
using iFakeLocation.Services.Mount;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Plist;
using iMobileDevice.PropertyListService;

namespace iFakeLocation
{
    class DeviceInformation
    {
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
            {"iPhone14,7", "iPhone 14"},
            {"iPhone14,8", "iPhone 14 Plus"},
            {"iPhone15,2", "iPhone 14 Pro"},
            {"iPhone15,3", "iPhone 14 Pro Max"},

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
            {"iPad13,17", "iPad Air 5th Gen (WiFi+Cellular)"},
            {"iPad13,18", "iPad 10th Gen"},
            {"iPad13,19", "iPad 10th Gen"},
            {"iPad14,3", "iPad Pro 11 inch 4th Gen"},
            {"iPad14,4", "iPad Pro 11 inch 4th Gen"},
            {"iPad14,5", "iPad Pro 12.9 inch 6th Gen"},
            {"iPad14,6", "iPad Pro 12.9 inch 6th Gen"}
        };

        public string Name { get; }
        public string UDID { get; }
        public bool IsNetwork { get; }
        public Dictionary<string, object> Properties { get; private set; }

        private DeviceInformation(string name, string udid, bool isNetwork)
        {
            Name = name;
            UDID = udid;
            IsNetwork = isNetwork;
            Properties = new Dictionary<string, object>();
        }

        private void ReadProperties(PlistHandle node)
        {
            Properties =
                PlistHelper.ReadPlistDictFromNode(node);
        }

        public override string ToString()
        {
            var sb = new StringBuilder().Append(Name).Append(" (");

            if (Properties.ContainsKey("ProductType"))
            {
                if (Properties["ProductType"] is string && RealProductName.ContainsKey((string)Properties["ProductType"]))
                    sb.Append(RealProductName[(string)Properties["ProductType"]]);
                else
                    sb.Append(Properties["ProductType"]);
            }

            if (Properties.ContainsKey("ProductVersion"))
                sb.Append("; iOS ").Append(Properties["ProductVersion"]);

            sb.Append(") [").Append(IsNetwork ? "Wi-Fi" : "USB").Append(']');
            return sb.ToString();
        }

        public enum DeveloperModeToggleState
        {
            NA,
            Visible,
            Hidden
        }

        public DeveloperModeToggleState GetDeveloperModeToggleState()
        {
            // Toggle only exists on iOS 16 onwards
            if (int.Parse(((string)Properties["ProductVersion"]).Split('.')[0]) < 16)
            {
                return DeveloperModeToggleState.NA;
            }

            iDeviceHandle deviceHandle = null;
            LockdownClientHandle lockdownHandle = null;
            PlistHandle plistHandle = null;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var plist = LibiMobileDevice.Instance.Plist;

            try
            {
                // Get device handle
                if (idevice.idevice_new_with_options(out deviceHandle, UDID, (int)(IsNetwork ? iDeviceOptions.LookupNetwork : iDeviceOptions.LookupUsbmux)) != iDeviceError.Success)
                    throw new Exception("Unable to open device, is it connected?");

                // Get lockdownd handle
                if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "iFakeLocation") !=
                    LockdownError.Success)
                {
                    throw new Exception("Unable to connect to lockdownd.");
                }

                // Check AMFI DeveloperModeStatus property
                if (lockdown.lockdownd_get_value(lockdownHandle, "com.apple.security.mac.amfi", "DeveloperModeStatus",
                        out plistHandle) !=
                    LockdownError.Success)
                {
                    throw new Exception("Unable to query com.apple.security.mac.amfi service.");
                }

                char status = '\0';
                plist.plist_get_bool_val(plistHandle, ref status);
                return status > 0 ? DeveloperModeToggleState.Visible : DeveloperModeToggleState.Hidden;
            }
            finally
            {
                if (plistHandle != null)
                    plistHandle.Close();

                if (lockdownHandle != null)
                    lockdownHandle.Close();

                if (deviceHandle != null)
                    deviceHandle.Close();
            }
        }

        public void EnableDeveloperModeToggle()
        {
            // Toggle only exists on iOS 16 onwards
            if (int.Parse(((string)Properties["ProductVersion"]).Split('.')[0]) < 16)
            {
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

            try
            {
                // Get device handle
                if (idevice.idevice_new_with_options(out deviceHandle, UDID, (int)(IsNetwork ? iDeviceOptions.LookupNetwork : iDeviceOptions.LookupUsbmux)) != iDeviceError.Success)
                    throw new Exception("Unable to open device, is it connected?");

                // Get lockdownd handle
                if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "iFakeLocation") !=
                    LockdownError.Success)
                {
                    throw new Exception("Unable to connect to lockdownd.");
                }

                // Start the com.apple.amfi.lockdown service
                if (lockdown.lockdownd_start_service(lockdownHandle, "com.apple.amfi.lockdown", out serviceDescriptor) !=
                    LockdownError.Success)
                {
                    throw new Exception("Unable to start the com.apple.amfi.lockdown service.");
                }

                // Create property list service client.
                if (propertyListService.property_list_service_client_new(deviceHandle, serviceDescriptor,
                        out propertyListServiceClientHandle) != PropertyListServiceError.Success)
                {
                    throw new Exception("Unable to create property list service client.");
                }

                // Create and send plist to the AMFI lockdown server
                plistHandle = plist.plist_new_dict();

                // 0 = reveal toggle in settings
                // 1 = enable developer mode (only if no passcode is set)
                // 2 = answers developer mode enable prompt post-restart?
                plist.plist_dict_set_item(plistHandle, "action", plist.plist_new_uint(0));

                if (propertyListService.property_list_service_send_xml_plist(propertyListServiceClientHandle,
                        plistHandle) != PropertyListServiceError.Success)
                {
                    throw new Exception("Failed to send request to enable developer mode toggle.");
                }
                plistHandle.Close();
                plistHandle = null;

                // Parse the response from the service
                if (propertyListService.property_list_service_receive_plist(propertyListServiceClientHandle,
                        out plistHandle) != PropertyListServiceError.Success)
                {
                    throw new Exception("Failed to retrieve response after attempting to enable developer mode toggle.");
                }

                var dict = PlistHelper.ReadPlistDictFromNode(plistHandle);
                if (dict.ContainsKey("Error"))
                {
                    throw new Exception("Failed to enable the developer mode toggle: " + dict["Error"]);
                }
                else if (dict.ContainsKey("success"))
                {
                    if (!(bool)dict["success"])
                    {
                        throw new Exception("Failed to enable the developer mode toggle (unknown error)");
                    }
                }
                else
                {
                    throw new Exception("Failed to enable the developer mode toggle (unexpected response)");
                }
            }
            finally
            {
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

        public void EnableDeveloperMode(string[] fileNames)
        {
            // Use personalized image mounter for iOS 17 and above, otherwise use the standard mobile image mounter
            if (int.Parse(((string)Properties["ProductVersion"]).Split('.')[0]) >= 17)
            {
                new PersonalizedImageMounter(this).EnableDeveloperMode(fileNames);
            }
            else
            {
                new DeveloperDiskImageMounter(this).EnableDeveloperMode(fileNames);
            }
        }

        public void StopLocation()
        {
            SetLocation(null);
        }

        public void SetLocation(PointLatLng? target)
        {
            // Use DVT for iOS 17 and above, otherwise use the standard DT service
            if (int.Parse(((string)Properties["ProductVersion"]).Split('.')[0]) >= 17)
            {
                throw new NotImplementedException("Setting location is currently not supported for iOS 17 or newer.");
            }
            else
            {
                new DtSimulateLocation(this).SetLocation(target);
            }
        }

        public static List<DeviceInformation> GetDevices(bool includeNetwork = true)
        {
            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var plist = LibiMobileDevice.Instance.Plist;

            var devices = new List<DeviceInformation>();

            // Obtain list of pointers to iDeviceInfo structures
            IntPtr devListPtr = IntPtr.Zero;
            int count = 0;
            try
            {
                if (idevice.idevice_get_device_list_extended(ref devListPtr, ref count) != iDeviceError.Success)
                    return null;

                iDeviceHandle deviceHandle = null;
                LockdownClientHandle lockdownHandle = null;
                PlistHandle plistHandle = null;

                // Ensure the iDeviceInfo* is not-null
                IntPtr devListCurPtr = devListPtr;
                while (devListCurPtr != IntPtr.Zero &&
                       Marshal.ReadIntPtr(devListCurPtr) != IntPtr.Zero)
                {

                    // Skip network devices if not enabled
                    iDeviceInfo info = (iDeviceInfo)Marshal.PtrToStructure(Marshal.ReadIntPtr(devListCurPtr), typeof(iDeviceInfo));
                    devListCurPtr = IntPtr.Add(devListCurPtr, IntPtr.Size);

                    bool isNetwork = info.conn_type == iDeviceConnectionType.Network;
                    if (isNetwork && !includeNetwork)
                        continue;

                    try
                    {
                        // Attempt to get device handle of each uuid
                        var err = idevice.idevice_new_with_options(out deviceHandle, info.udidString,
                            (int)(isNetwork ? iDeviceOptions.LookupNetwork : iDeviceOptions.LookupUsbmux));
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

                        device = new DeviceInformation(name, info.udidString, isNetwork);

                        // Get device details
                        if (lockdown.lockdownd_get_value(lockdownHandle, null, null, out plistHandle) !=
                            LockdownError.Success ||
                            plist.plist_get_node_type(plistHandle) != PlistType.Dict)
                            continue;

                        device.ReadProperties(plistHandle);

                        // Ensure device is attached
                        if (!device.Properties.ContainsKey("HostAttached") || (bool)device.Properties["HostAttached"])
                            devices.Add(device);
                    }
                    finally
                    {
                        // Cleanup
                        if (plistHandle != null)
                            plistHandle.Close();
                        if (lockdownHandle != null)
                            lockdownHandle.Close();
                        if (deviceHandle != null)
                            deviceHandle.Close();
                    }
                }
            }
            finally
            {
                if (devListPtr != IntPtr.Zero)
                    idevice.idevice_device_list_extended_free(devListPtr);
            }


            return devices;
        }
    }
}