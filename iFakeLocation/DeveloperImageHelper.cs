using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;

namespace iFakeLocation {
    static class DeveloperImageHelper {
        private class WebClientEx : WebClient {
            protected override WebRequest GetWebRequest(Uri address) {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                if (request != null)
                    request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }

        private static readonly WebClient WebClient = new WebClientEx();
        private static readonly Dictionary<string, string> VersionToImageUrlLegacy = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> VersionToImageUrlZip = new Dictionary<string, string>();

        private const string ImagePath = "DeveloperImages";

        public static bool HasImageForDevice(DeviceInformation device) {
            string[] p;
            return HasImageForDevice(device, out p);
        }

        private static readonly Dictionary<string, string> VersionMapping = new Dictionary<string, string> {
            {"12.4", "12.3"}
        };

        public static string GetSoftwareVersion(DeviceInformation device) {
            var ver = ((string) device.Properties["ProductVersion"]).Split('.');
            string v = ver[0] + "." + ver[1];
            return VersionMapping.ContainsKey(v) ? VersionMapping[v] : v;
        }

        public static bool HasImageForDevice(DeviceInformation device, out string[] paths) {
            var verStr = GetSoftwareVersion(device);
            var a = Path.Combine(ImagePath, verStr, "DeveloperDiskImage.dmg");
            var b = a + ".signature";
            paths = new[] {a, b};
            return File.Exists(a) && File.Exists(b);
        }

        public static Tuple<string, string>[] GetLinksForDevice(DeviceInformation device) {
            string verStr = GetSoftwareVersion(device);

            // Populate URLs for developer images from Github
            if (VersionToImageUrlLegacy.Count == 0) {
                string treeList = "795fc91f28cb3884edc45b876482911c797de85c";
                try {
                    WebClient.Headers["X-Requested-With"] = "XMLHttpRequest";
                    var resp = WebClient.DownloadString(
                        "https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/find/master?_pjax=%23js-repo-pjax-container");
                    var tl = "/tree-list/";
                    var idx = resp.IndexOf(tl, StringComparison.InvariantCultureIgnoreCase);
                    if (idx != -1)
                        treeList = resp.Substring(idx + tl.Length, resp.IndexOf('\"', idx) - (idx + tl.Length));
                }
                catch {
                }

                WebClient.Headers["Accept"] = "application/json";
                WebClient.Headers["X-Requested-With"] = "XMLHttpRequest";
                var response =
                    WebClient.DownloadString("https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/tree-list/" +
                                             treeList);
                var paths = response.Split('"')
                    .Where(s => s.EndsWith(".dmg", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                foreach (var path in paths)
                    VersionToImageUrlLegacy[path.Split('/')[1].Split(' ')[0]] =
                        "https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/raw/master/" + path;
            }

            if (VersionToImageUrlZip.Count == 0) {
                string treeList = "89cdf804bd416d0d6ba3f958b5c6d086cb914fa1";
                try {
                    WebClient.Headers["X-Requested-With"] = "XMLHttpRequest";
                    var resp = WebClient.DownloadString(
                        "https://github.com/haikieu/xcode-developer-disk-image-all-platforms/find/master?_pjax=%23js-repo-pjax-container");
                    var tl = "/tree-list/";
                    var idx = resp.IndexOf(tl, StringComparison.InvariantCultureIgnoreCase);
                    if (idx != -1)
                        treeList = resp.Substring(idx + tl.Length, resp.IndexOf('\"', idx) - (idx + tl.Length));
                }
                catch {
                }

                WebClient.Headers["Accept"] = "application/json";
                WebClient.Headers["X-Requested-With"] = "XMLHttpRequest";
                var response =
                    WebClient.DownloadString("https://github.com/haikieu/xcode-developer-disk-image-all-platforms/tree-list/" +
                                             treeList);
                var paths = response.Split('"')
                    .Where(s => s.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                foreach (var path in paths)
                    VersionToImageUrlZip[Path.GetFileNameWithoutExtension(path.Split('/').Last())] =
                        "https://github.com/haikieu/xcode-developer-disk-image-all-platforms/raw/master/" + path;
            }

            if (VersionToImageUrlLegacy.TryGetValue(verStr, out var ss)) {
                return new[] {
                    new Tuple<string, string>(ss, Path.Combine(ImagePath, verStr, "DeveloperDiskImage.dmg")),
                    new Tuple<string, string>(ss + ".signature",
                        Path.Combine(ImagePath, verStr, "DeveloperDiskImage.dmg.signature"))
                };
            }

            if (VersionToImageUrlZip.TryGetValue(verStr, out ss)) {
                return new[] {
                    new Tuple<string, string>(ss, Path.Combine(ImagePath, verStr, verStr + ".zip"))
                };
            }

            return null;
        }
    }
}