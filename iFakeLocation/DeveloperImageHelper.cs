using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;

namespace iFakeLocation
{
    static class DeveloperImageHelper {
        static DeveloperImageHelper() {
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        private class WebClientEx : WebClient {
            protected override WebRequest GetWebRequest(Uri address) {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                if (request != null) request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }

        private static readonly WebClient WebClient = new WebClientEx();
        private static readonly Dictionary<string, string> VersionToImageUrl = new Dictionary<string, string>();

        private const string ImagePath = "DeveloperImages";

        public static bool HasImageForDevice(DeviceInformation device) {
            string[] p;
            return HasImageForDevice(device, out p);
        }

        public static bool HasImageForDevice(DeviceInformation device, out string[] paths) {
            var ver = ((string)device.Properties["ProductVersion"]).Split('.');
            var verStr = ver[0] + "." + ver[1];

            var a = Path.Combine(ImagePath, verStr, "DeveloperDiskImage.dmg");
            var b = a + ".signature";
            paths = new[] {a, b};
            return File.Exists(a) && File.Exists(b);
        }

        public static Tuple<string, string>[] GetLinksForDevice(DeviceInformation device) {
            var ver = ((string) device.Properties["ProductVersion"]).Split('.');
            var verStr = ver[0] + "." + ver[1];

            // Populate URLs for developer images from Github
            if (VersionToImageUrl.Count == 0)
            {
                WebClient.Headers["Accept"] = "application/json";
                WebClient.Headers["X-Requested-With"] = "XMLHttpRequest";
                var response = WebClient.DownloadString("https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/tree-list/6718c0ced4dadf28ecc8411fddce32f4f779db9f");
                var paths = response.Split('"').Where(s => s.EndsWith(".dmg", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                foreach (var path in paths)
                    VersionToImageUrl.Add(path.Split('/')[1].Split(' ')[0], "https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/raw/master/" + path);
            }

            string ss;
            return VersionToImageUrl.TryGetValue(verStr, out ss) ? new[] {
                new Tuple<string, string>(ss, Path.Combine(ImagePath, verStr, "DeveloperDiskImage.dmg")),
                new Tuple<string, string>(ss + ".signature", Path.Combine(ImagePath, verStr, "DeveloperDiskImage.dmg.signature"))
            } : null;
        }
    }
}
