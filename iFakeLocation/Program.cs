using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Net.Http;
using iMobileDevice;
using Newtonsoft.Json;
using ICSharpCode.SharpZipLib.Zip;
using System.Runtime.InteropServices;

namespace iFakeLocation
{
    class Program {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        class EndpointMethod : Attribute {
            public string Name { get; }

            public EndpointMethod(string name) {
                Name = name;
            }
        }

        static bool TryBindListenerOnFreePort(out HttpListener httpListener, out int port) {
            // IANA suggested range for dynamic or private ports
            const int MinPort = 49215;
            const int MaxPort = 65535;

            for (port = MinPort; port < MaxPort; port++) {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://localhost:{port}/");
                try {
                    httpListener.Start();
                    return true;
                }
                catch {
                }
            }

            port = 0;
            httpListener = null;
            return false;
        }

        static void OpenBrowser(string url) {
            try {
                Process.Start(url);
            }
            catch {
#if NET
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    Process.Start("open", url);
                }
                else {
#endif
                    throw;
#if NET
                }
#endif
            }
        }

        static byte[] ReadStream(Stream stream) {
            using (var ms = new MemoryStream()) {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        static void SetResponse(HttpListenerContext ctx, string response) {
            using (var sw = new StreamWriter(ctx.Response.OutputStream))
                sw.Write(response);
        }

        static void SetResponse(HttpListenerContext ctx, object response) {
            using (var sw = new StreamWriter(ctx.Response.OutputStream))
                sw.Write(JsonConvert.SerializeObject(response));
        }

        [EndpointMethod("version")]
        static void Version(HttpListenerContext ctx) {
            // Write version as response
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            SetResponse(ctx, v.Major + "." + v.Minor);
        }

        [EndpointMethod("home_country")]
        static void HomeCountry(HttpListenerContext ctx) {
            // Write current region's english name as response
            SetResponse(ctx, RegionInfo.CurrentRegion.EnglishName);
        }

        private static List<DeviceInformation> Devices = new List<DeviceInformation>();

        [EndpointMethod("get_devices")]
        static void GetDevices(HttpListenerContext ctx) {
            // Save current devices
            try {
                if (Devices != null)
                    lock (Devices)
                        Devices = DeviceInformation.GetDevices();
            }
            catch (Exception e) {
                SetResponse(ctx, new {
                    error = e.Message
                });
            }

            // No devices could be read, sent error
            if (Devices == null) {
                SetResponse(ctx, new {
                    error =
                        "Unable to retrieve connected devices. Ensure iTunes is installed and can detect your device(s)."
                });
            }
            else {
                // Write devices to output
                SetResponse(ctx,
                    Devices.Select(d => new {
                        name = d.Name,
                        display_name = d.ToString(),
                        udid = d.UDID
                    })
                );
            }
        }

        class DownloadState {
            public string[] Links { get; }
            public string[] Paths { get; }
            public int CurrentIndex { get; set; }
            public float Progress { get; set; }
            public Exception Error { get; set; }
            public bool Done { get; set; }
            public HttpClient HttpClient { get; }

            public event EventHandler<EventArgs> DownloadCompleted;

            public DownloadState(string[] links, string[] paths) {
                Links = links;
                Paths = paths;
                HttpClient = new HttpClient();
            }

            private void DownloadFileCompleted(Exception e) {
                if (e != null) {
                    Error = e;
                }
                else {
                    try {
                        if (File.Exists(Paths[CurrentIndex])) File.Delete(Paths[CurrentIndex]);
                        File.Move(Paths[CurrentIndex] + ".incomplete", Paths[CurrentIndex]);
                    }
                    catch (Exception ex) {
                        Error = ex;
                        return;
                    }

                    if (CurrentIndex + 1 >= Links.Length) {
                        DownloadCompleted?.Invoke(this, EventArgs.Empty);
                        Done = true;
                    }
                    else {
                        CurrentIndex++;
                        ProcessNext();
                    }
                }
            }

            private void DownloadAsync(Uri uri, string destinationPath) {
                HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
                    .ContinueWith(response => {
                        if (response.IsFaulted) {
                            DownloadFileCompleted(response.Exception);
                            return;
                        }

                        var contentLength = response.Result.Content.Headers.ContentLength;
                        response.Result.Content.ReadAsStreamAsync().ContinueWith(result => {
                            if (result.IsFaulted) {
                                DownloadFileCompleted(result.Exception);
                                return;
                            }

                            var stream = result.Result;

                            // Check if progress reporting can be done
                            try {
                                using (var destStream = File.OpenWrite(destinationPath)) {
                                    if (!contentLength.HasValue) {
                                        stream.CopyTo(destStream);
                                        DownloadFileCompleted(null);
                                        return;
                                    }

                                    // Download the file and report ongoing progress
                                    var buffer = new byte[8192];
                                    long totalBytesRead = 0;
                                    int bytesRead;
                                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) {
                                        destStream.Write(buffer, 0, bytesRead);
                                        totalBytesRead += bytesRead;
                                        Progress = (float)totalBytesRead / contentLength.Value * 100.0f;
                                    }
                                }
                                
                                DownloadFileCompleted(null);
                            }
                            catch (Exception ex) {
                                DownloadFileCompleted(ex);
                            }
                        });
                    });
            }

            private void ProcessNext() {
                Progress = 0;
                var p = Path.GetDirectoryName(Paths[CurrentIndex]);
                if (!string.IsNullOrEmpty(p) && !Directory.Exists(p))
                    Directory.CreateDirectory(p);
                DownloadAsync(new Uri(Links[CurrentIndex]), Paths[CurrentIndex] + ".incomplete");
            }

            public void Start() {
                if (CurrentIndex < Links.Length)
                    ProcessNext();
            }
        }

        static readonly Dictionary<string, DownloadState> Downloads = new Dictionary<string, DownloadState>();

        [EndpointMethod("get_progress")]
        static void GetProgress(HttpListenerContext ctx) {
            string version;
            using (var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                version = sr.ReadToEnd();

            if (Downloads.TryGetValue(version, out DownloadState state)) {
                if (state.Error != null) {
                    SetResponse(ctx, new { error = state.Error.ToString() });
                }
                else if (state.Done) {
                    SetResponse(ctx, new { done = true });
                }
                else {
                    SetResponse(ctx,
                        new { filename = Path.GetFileName(state.Paths[state.CurrentIndex]), progress = state.Progress });
                }
            }
            else {
                SetResponse(ctx, new { error = "Download state is unrecognised." });
            }
        }

        [EndpointMethod("stop_location")]
        static void StopLocation(HttpListenerContext ctx) {
            if (ctx.Request.Headers["Content-Type"] == "application/json") {
                using (var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding)) {
                    // Read the JSON body
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd());
                    DeviceInformation device;

                    // Find the matching device udid
                    lock (Devices)
                        device = Devices.FirstOrDefault(d => d.UDID == (string) data.udid);

                    // Check if we already have the dependencies
                    if (device == null) {
                        SetResponse(ctx,
                            new {error = "Unable to find the specified device. Are you sure it is connected?"});
                    }
                    else {
                        try {
                            if (DeveloperImageHelper.HasImageForDevice(device, out string[] p)) {
                                device.EnableDeveloperMode(p[0], p[1]);
                                device.StopLocation();
                                SetResponse(ctx, new { success = true });
                            }
                            else {
                                throw new Exception("The developer images for the specified device are missing.");
                            }
                        }
                        catch (Exception e) {
                            SetResponse(ctx, new {error = e.Message});
                        }
                    }
                }
            }
        }

        [EndpointMethod("set_location")]
        static void SetLocation(HttpListenerContext ctx) {
            if (ctx.Request.Headers["Content-Type"] == "application/json") {
                using (var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding)) {
                    // Read the JSON body
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd());
                    DeviceInformation device;

                    // Find the matching device udid
                    lock (Devices)
                        device = Devices.FirstOrDefault(d => d.UDID == (string) data.udid);

                    // Check if we already have the dependencies
                    if (device == null) {
                        SetResponse(ctx,
                            new {error = "Unable to find the specified device. Are you sure it is connected?"});
                    }
                    else {
                        try {
                            // Check if developer mode toggle is visible (on >= iOS 16)
                            if (device.GetDeveloperModeToggleState() ==
                                DeviceInformation.DeveloperModeToggleState.Hidden) {
                                device.EnableDeveloperModeToggle();
                                SetResponse(ctx,
                                    new {
                                        error = "Please turn on Developer Mode first via Settings >> Privacy & Security on your device."
                                    });
                            }
                            // Ensure the developer image exists
                            else if (DeveloperImageHelper.HasImageForDevice(device, out var p)) {
                                device.EnableDeveloperMode(p[0], p[1]);
                                device.SetLocation(new PointLatLng {Lat = data.lat, Lng = data.lng});
                                SetResponse(ctx, new {success = true});
                            }
                            else {
                                throw new Exception("The developer images for the specified device are missing.");
                            }
                        }
                        catch (Exception e) {
                            SetResponse(ctx, new {error = e.Message});
                        }
                    }
                }
            }
        }

        [EndpointMethod("exit")]
        static void Exit(HttpListenerContext ctx) {
            SetResponse(ctx, "");
            Environment.Exit(0);
        }

        static void DeveloperImageZipDownloaded(object sender, EventArgs e) {
            var state = (DownloadState) sender;
            var files = state.Paths;

            try {
                foreach (var file in files) {
                    using (var fs = File.OpenRead(file)) {
                        using (var zf = new ZipFile(fs)) {
                            foreach (ZipEntry ze in zf) {
                                if (!ze.IsFile || !ze.Name.Contains("DeveloperDiskImage.dmg"))
                                    continue;
                                using (var ds = zf.GetInputStream(ze)) {
                                    var dest = Path.Combine(Path.GetDirectoryName(file),
                                        ze.Name.Replace('\\', '/').Split('/').Last());
                                    using (var of = File.OpenWrite(dest))
                                        ds.CopyTo(of);
                                }
                            }
                        }
                    }

                    File.Delete(file);
                }
            }
            catch (Exception ex) {
                state.Error = ex;
            }
        }

        [EndpointMethod("has_dependencies")]
        static void HasDepedencies(HttpListenerContext ctx) {
            if (ctx.Request.Headers["Content-Type"] == "application/json") {
                using (var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding)) {
                    // Read the JSON body
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd());
                    DeviceInformation device;

                    // Find the matching device udid
                    lock (Devices)
                        device = Devices.FirstOrDefault(d => d.UDID == (string) data.udid);

                    // Check if we already have the dependencies
                    if (device == null) {
                        SetResponse(ctx,
                            new {error = "Unable to find the specified device. Are you sure it is connected?"});
                    }
                    else {
                        // Obtain the status of the depedencies
                        var hasDeps = DeveloperImageHelper.HasImageForDevice(device);
                        var verStr = DeveloperImageHelper.GetSoftwareVersion(device);

                        // Automatically start download if it's missing
                        if (!hasDeps) {
                            var links = DeveloperImageHelper.GetLinksForDevice(device);
                            if (links != null) {
                                bool needsExtraction = links.Any(l =>
                                    l.Item1.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase));
                                var state = new DownloadState(links.Select(t => t.Item1).ToArray(),
                                    links.Select(t => t.Item2).ToArray());
                                if (needsExtraction)
                                    state.DownloadCompleted += DeveloperImageZipDownloaded;
                                lock (Downloads)
                                    if (!Downloads.ContainsKey(verStr))
                                        Downloads[verStr] = state;
                                state.Start();
                            }
                            else {
                                SetResponse(ctx,
                                    new {error = "Your device's iOS version is not supported at this time."});
                                return;
                            }
                        }

                        SetResponse(ctx, new {result = hasDeps, version = verStr});
                    }
                }
            }
        }

        static void Main() {
            // Configure paths
            string basePath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Environment.CurrentDirectory = basePath;

            foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType))) {
                try {
                    ServicePointManager.SecurityProtocol |= protocol;
                }
                catch {
                }
            }

            try {
                NativeLibraries.Load();
            }
            catch {
                Console.WriteLine("Failed to load necessary files to run iFakeLocation.");
                return;
            }

            // Retrieve all web endpoint methods
            var methods =
                typeof(Program).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Select(mi => new Tuple<MethodInfo, object>(mi,
                        mi.GetCustomAttributes(true).FirstOrDefault(ci => ci is EndpointMethod)))
                    .Where(kvp => kvp.Item2 != null)
                    .ToDictionary(kvp => ((EndpointMethod) kvp.Item2).Name, kvp => kvp.Item1);

            // Find a free port to run our local server on
            if (!TryBindListenerOnFreePort(out var listener, out var port)) {
                Console.WriteLine("Failed to initialise iFakeLocation (no free ports on local system).");
                return;
            }

            // Start window
            try {
                OpenBrowser($"http://localhost:{port}/");
                Console.WriteLine("iFakeLocation is now running at: " + $"http://localhost:{port}/");
                Console.WriteLine("\nPress Ctrl-C to quit (or click the close button).");
            }
            catch {
                Console.WriteLine("Unable to start iFakeLocation using default web browser.");
                return;
            }

            // Main processing loop
            while (true) {
                var ctx = listener.GetContext();
                ThreadPool.QueueUserWorkItem(_ => {
                    // Extract the method name from the URL
                    var methodName = ctx.Request.Url.Segments.Length > 1
                        ? string.Join("", ctx.Request.Url.Segments.Skip(1))
                        : "";
                    if (string.IsNullOrEmpty(methodName))
                        methodName = "main.html";

                    // Respond with static resource if specified
                    string path;
                    if (File.Exists(path = Path.Combine("Resources",
                        methodName.Replace('/', Path.DirectorySeparatorChar)))) {
                        ctx.Response.Headers["Content-Type"] = MimeTypes.GetMimeType(methodName);
                        using (var s = File.OpenRead(path))
                            s.CopyTo(ctx.Response.OutputStream);
                        ctx.Response.Close();
                        return;
                    }
                    // Response with response from web method
                    else if (methods.TryGetValue(methodName, out MethodInfo method)) {
                        try {
                            method.Invoke(null, new object[] {ctx});
                        }
                        catch (Exception e) {
                            Console.WriteLine("\n" + e);
                        }

                        try {
                            if (ctx.Response.OutputStream.CanWrite)
                                ctx.Response.OutputStream.Close();
                        }
                        catch (ObjectDisposedException) {
                        }
                    }
                    else {
                        // Response with nothing
                        ctx.Response.Close();
                        return;
                    }
                });
            }
        }
    }
}
