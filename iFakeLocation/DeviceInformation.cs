﻿using System.Text;
using System.Text.Json;

namespace iFakeLocation;

internal class DeviceInformation
{
    private static readonly Dictionary<string, string> RealProductName = new()
    {
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

    public required string DeviceName { get; set; }
    public required string Identifier { get; set; }
    public string? ConnectionType { get; set; }
    public bool IsNetwork => ConnectionType != "USB";

    public string? BuildVersion { get; set; }
    public string? DeviceClass { get; set; }
    public string? ProductType { get; set; }
    public string? ProductVersion { get; set; }
    public bool IsIos17OrHigher => int.Parse(ProductVersion?.Split('.')[0] ?? "0") >= 17;

    public override string ToString()
    {
        var sb = new StringBuilder().Append(DeviceName).Append(" (");

        if (ProductType != null)
        {
            sb.Append(RealProductName.GetValueOrDefault(ProductType, ProductType));
        }

        if (ProductVersion != null)
            sb.Append("; iOS ").Append(ProductVersion);

        sb.Append(") [").Append(ConnectionType).Append(']');
        return sb.ToString();
    }

    public void StopLocation()
    {
        PyMobileDevice.ClearLocation(this);
    }

    public async Task SetLocation(Records target)
    {
        var location = $"{target.Lat} {target.Lng}";
        await PyMobileDevice.SetLocation(this, location);
    }

    public static DeviceInformation[]? GetDevices(bool includeNetwork = true)
    {
        var devicesJson = PyMobileDevice.GetDevices();

        var devices = JsonSerializer.Deserialize<DeviceInformation[]>(devicesJson);

        if (devices?.Length == 0)
        {
            throw new Exception("No devices found. Please make sure you have a device connected, unlocked and visible in iTunes.");
        }

        return devices;
    }
}