using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace iFakeLocation {
    static class DeveloperImageHelper {
        private static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
        private static readonly Dictionary<string, string> VersionToImageUrlLegacy = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> VersionToImageUrlZip = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> VersionToImageUrlOverride = new Dictionary<string, string>();

        private const string ImagePath = "DeveloperImages";

        private static readonly string[] MobileImageFileList = new[] {
            "DeveloperDiskImage.dmg",
            "DeveloperDiskImage.dmg.signature"
        };

        private static readonly string[] PersonalisedImageFileList = new[] {
            "Image.dmg",
            "BuildManifest.plist",
            "Image.dmg.trustcache"
        };

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

        public static bool IsKnownImageFileName(string fileName) {
            fileName = fileName.Split('/', '\\').Last();
            return MobileImageFileList.Contains(fileName) || PersonalisedImageFileList.Contains(fileName);
        }

        private static bool ImageExists(string[] paths) {
            return paths.All(File.Exists);
        }

        public static bool HasImageForDevice(DeviceInformation device, out string[] paths) {
            var verStr = GetSoftwareVersion(device);

            var expectedFiles = int.Parse(verStr.Split('.')[0]) >= 17
                ? PersonalisedImageFileList.Select(p => (object)Path.Combine(ImagePath, verStr, p)).ToArray()
                : MobileImageFileList.Select(p => (object)Path.Combine(ImagePath, verStr, p)).ToArray();
            
            var s = Path.DirectorySeparatorChar;
            string[][] pathPatterns = new[] {
                new[] { "{0}", "{1}", "{2}" },
                new[] { $".{s}..{s}{{0}}", $".{s}..{s}{{1}}", $".{s}..{s}{{2}}" },
                new[] { $".{s}..{s}..{s}{{0}}", $".{s}..{s}..{s}{{1}}", $".{s}..{s}..{s}{{2}}" }
            };

            foreach (var pathPattern in pathPatterns) {
                if (ImageExists(paths = pathPattern.Take(expectedFiles.Length)
                        .Select(pattern => string.Format(pattern, expectedFiles)).ToArray())) {
                    return true;
                }
            }

            paths = null;
            return false;
        }

        public static Tuple<string, string>[] GetLinksForDevice(DeviceInformation device) {
            string verStr = GetSoftwareVersion(device);

            // Populate URLs for developer images from Github
            if (VersionToImageUrlLegacy.Count == 0) {
                string treeList = "795fc91f28cb3884edc45b876482911c797de85c";
                try {
                    HttpClient.DefaultRequestHeaders.Clear();
                    HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                    HttpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    var resp = HttpClient.GetStringAsync(
                        "https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/find/master?_pjax=%23js-repo-pjax-container").Result;
                    var tl = "/tree-list/";
                    var idx = resp.IndexOf(tl, StringComparison.InvariantCultureIgnoreCase);
                    if (idx != -1)
                        treeList = resp.Substring(idx + tl.Length, resp.IndexOf('\"', idx) - (idx + tl.Length));
                }
                catch {
                }

                try {
                    HttpClient.DefaultRequestHeaders.Clear();
                    HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                    HttpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    var response =
                        HttpClient.GetStringAsync(
                            "https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/tree-list/" +
                            treeList).Result;
                    var paths = response.Split('"')
                        .Where(s => s.EndsWith(".dmg", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                    foreach (var path in paths)
                        VersionToImageUrlLegacy[path.Split('/')[1].Split(' ')[0]] =
                            "https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/raw/master/" + path;
                } catch {
                }
            }

            if (VersionToImageUrlZip.Count == 0) {
                string treeList = "89cdf804bd416d0d6ba3f958b5c6d086cb914fa1";
                try {
                    HttpClient.DefaultRequestHeaders.Clear();
                    HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                    HttpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    var resp = HttpClient.GetStringAsync(
                            "https://github.com/haikieu/xcode-developer-disk-image-all-platforms/find/master?_pjax=%23js-repo-pjax-container")
                        .Result;
                    var tl = "/tree-list/";
                    var idx = resp.IndexOf(tl, StringComparison.InvariantCultureIgnoreCase);
                    if (idx != -1)
                        treeList = resp.Substring(idx + tl.Length, resp.IndexOf('\"', idx) - (idx + tl.Length));
                }
                catch {
                }

                try {
                    HttpClient.DefaultRequestHeaders.Clear();
                    HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                    HttpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    var response =
                        HttpClient.GetStringAsync(
                            "https://github.com/haikieu/xcode-developer-disk-image-all-platforms/tree-list/" +
                            treeList).Result;
                    var paths = response.Split('"')
                        .Where(s => s.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) &&
                                          s.IndexOf("iPhoneOS", StringComparison.InvariantCulture) >= 0).ToArray();
                    foreach (var path in paths)
                        VersionToImageUrlZip[Path.GetFileNameWithoutExtension(path.Split('/').Last())] =
                            "https://github.com/haikieu/xcode-developer-disk-image-all-platforms/raw/master/" + path;
                } catch {
                }
            }

            // Use special override source that is under control of author and can be updated without
            // issuing a client update.
            // "<iOS version>": [".zip URL source containing developer image and signature"]
		    // or
		    // "<iOS version>": [".dmg URL source"] (.dmg.signature must also exist at the same path)
		
            if (VersionToImageUrlOverride.Count == 0) {
                try {
                    HttpClient.DefaultRequestHeaders.Clear();
                    HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                    var response =
                        JObject.Parse(HttpClient.GetStringAsync("https://raw.githubusercontent.com/master131/iFakeLocation/master/updates.json").Result);
                    foreach (var kvp in response.SelectToken("images").ToObject<Dictionary<string, string>>())
                        VersionToImageUrlOverride.Add(kvp.Key, kvp.Value);
                } catch {
                }
            }

            if (VersionToImageUrlOverride.TryGetValue(verStr, out var ss) ||
                VersionToImageUrlZip.TryGetValue(verStr, out ss) ||
                VersionToImageUrlLegacy.TryGetValue(verStr, out ss)) {
                if (ss.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase)) {
                    return new[] {
                        new Tuple<string, string>(ss, Path.Combine(ImagePath, verStr, verStr + ".zip"))
                    };
                } else {
                    return new[] {
                        new Tuple<string, string>(ss, Path.Combine(ImagePath, verStr, "DeveloperDiskImage.dmg")),
                        new Tuple<string, string>(ss + ".signature",
                            Path.Combine(ImagePath, verStr, "DeveloperDiskImage.dmg.signature"))
                    };
                }
            }

            return null;
        }
    }
}