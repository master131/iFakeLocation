using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Collections.Generic;
using iMobileDevice;
using Newtonsoft.Json;

namespace iFakeLocation
{
    class Program
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        class EndpointMethod : Attribute
        {
            public string Name { get; }

            public EndpointMethod(string name)
            {
                Name = name;
            }
        }

        static bool TryBindListenerOnFreePort(out HttpListener httpListener, out int port)
        {
            // IANA suggested range for dynamic or private ports
            const int MinPort = 49215;
            const int MaxPort = 65535;

            for (port = MinPort; port < MaxPort; port++)
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://localhost:{port}/");
                try
                {
                    httpListener.Start();
                    return true;
                }
                catch
                {
                }
            }

            port = 0;
            httpListener = null;
            return false;
        }

        static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
#if NETCOREAPP2_2
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
#endif
                    throw;
#if NETCOREAPP2_2
                }
#endif
            }
        }

        static byte[] ReadStream(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        static void SetResponse(HttpListenerContext ctx, string response)
        {
            using (var sw = new StreamWriter(ctx.Response.OutputStream))
                sw.Write(response);
        }

        static void SetResponse(HttpListenerContext ctx, object response)
        {
            using (var sw = new StreamWriter(ctx.Response.OutputStream))
                sw.Write(JsonConvert.SerializeObject(response));
        }

        [EndpointMethod("version")]
        static void Version(HttpListenerContext ctx)
        {
            // Write version as response
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            SetResponse(ctx, v.Major + "." + v.Minor);
        }

        [EndpointMethod("home_country")]
        static void HomeCountry(HttpListenerContext ctx)
        {
            // Write current region's english name as response
            SetResponse(ctx, RegionInfo.CurrentRegion.EnglishName);
        }

        private static List<DeviceInformation> Devices = new List<DeviceInformation>();

        [EndpointMethod("get_devices")]
        static void GetDevices(HttpListenerContext ctx)
        {
            // Save current devices
            try
            {
                if (Devices != null)
                    lock (Devices)
                        Devices = DeviceInformation.GetDevices();
            }
            catch (Exception e)
            {
                SetResponse(ctx, new {
                    error = e.Message
                });
            }

            // No devices could be read, sent error
            if (Devices == null)
            {
                SetResponse(ctx, new {
                    error = "Unable to retrieve connected devices. Ensure iTunes is installed and can detect your device(s)."
                });
            }
            else
            {
                // Write devices to output
                SetResponse(ctx,
                    Devices.Select(d => new
                    {
                        name = d.Name,
                        display_name = d.ToString(),
                        udid = d.UDID
                    })
                );
            }
        }

        class DownloadState
        {
            public string[] Links { get; }
            public string[] Paths { get; }
            public int CurrentIndex { get; set; }
            public float Progress { get; set; }
            public Exception Error { get; set; }
            public bool Done { get; set; }
            public WebClient WebClient { get; }

            public DownloadState(string[] links, string[] paths)
            {
                Links = links;
                Paths = paths;
                WebClient = new WebClient();
                WebClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                WebClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            }

            private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    Error = e.Error;
                }
                else
                {
                    try
                    {
                        File.Move(Paths[CurrentIndex] + ".incomplete", Paths[CurrentIndex]);
                    }
                    catch (Exception ex)
                    {
                        Error = ex;
                        return;
                    }

                    if (CurrentIndex + 1 >= Links.Length)
                    {
                        Done = true;
                    }
                    else
                    {
                        CurrentIndex++;
                        ProcessNext();
                    }
                }
            }

            private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                Progress = e.ProgressPercentage;
            }

            private void ProcessNext()
            {
                Progress = 0;
                var p = Path.GetDirectoryName(Paths[CurrentIndex]);
                if (!string.IsNullOrEmpty(p) && !Directory.Exists(p))
                    Directory.CreateDirectory(p);
                WebClient.DownloadFileAsync(new Uri(Links[CurrentIndex]), Paths[CurrentIndex] + ".incomplete");
            }

            public void Start()
            {
                if (CurrentIndex < Links.Length)
                    ProcessNext();
            }

            public void Stop()
            {
                WebClient.CancelAsync();
            }
        }

        static readonly Dictionary<string, DownloadState> Downloads = new Dictionary<string, DownloadState>();

        [EndpointMethod("get_progress")]
        static void GetProgress(HttpListenerContext ctx)
        {
            string version;
            using (var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                version = sr.ReadToEnd();

            DownloadState state;
            if (Downloads.TryGetValue(version, out state))
            {
                if (state.Error != null)
                {
                    SetResponse(ctx, new { error = state.Error.ToString() });
                }
                else if (state.Done)
                {
                    SetResponse(ctx, new { done = true });
                }
                else
                {
                    SetResponse(ctx, new { filename = Path.GetFileName(state.Paths[state.CurrentIndex]), progress = state.Progress });
                }
            }
            else
            {
                SetResponse(ctx, new { error = "Download state is unrecognised." });
            }
        }

        [EndpointMethod("stop_location")]
        static void StopLocation(HttpListenerContext ctx)
        {
            if (ctx.Request.Headers["Content-Type"] == "application/json")
            {
                using (var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    // Read the JSON body
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd());
                    DeviceInformation device;

                    // Find the matching device udid
                    lock (Devices)
                        device = Devices.FirstOrDefault(d => d.UDID == (string)data.udid);

                    // Check if we already have the dependencies
                    if (device == null)
                    {
                        SetResponse(ctx, new { error = "Unable to find the specified device. Are you sure it is connected?" });
                    }
                    else
                    {
                        try
                        {
                            string[] p;
                            if (DeveloperImageHelper.HasImageForDevice(device, out p))
                            {
                                device.EnableDeveloperMode(p[0], p[1]);
                                device.StopLocation();
                                SetResponse(ctx, new { success = true });
                            }
                            else
                            {
                                throw new Exception("The developer images for the specified device are missing.");
                            }
                        }
                        catch (Exception e)
                        {
                            SetResponse(ctx, new { error = e.Message });
                        }
                    }
                }
            }
        }

        [EndpointMethod("set_location")]
        static void SetLocation(HttpListenerContext ctx)
        {
            if (ctx.Request.Headers["Content-Type"] == "application/json")
            {
                using (var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    // Read the JSON body
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd());
                    DeviceInformation device;

                    // Find the matching device udid
                    lock (Devices)
                        device = Devices.FirstOrDefault(d => d.UDID == (string)data.udid);

                    // Check if we already have the dependencies
                    if (device == null)
                    {
                        SetResponse(ctx, new { error = "Unable to find the specified device. Are you sure it is connected?" });
                    }
                    else
                    {
                        try
                        {
                            string[] p;
                            if (DeveloperImageHelper.HasImageForDevice(device, out p))
                            {
                                device.EnableDeveloperMode(p[0], p[1]);
                                device.SetLocation(new PointLatLng { Lat = data.lat, Lng = data.lng });
                                SetResponse(ctx, new { success = true });
                            }
                            else
                            {
                                throw new Exception("The developer images for the specified device are missing.");
                            }
                        }
                        catch (Exception e)
                        {
                            SetResponse(ctx, new { error = e.Message });
                        }
                    }
                }
            }
        }

        [EndpointMethod("exit")]
        static void Exit(HttpListenerContext ctx)
        {
            SetResponse(ctx, "");
            Environment.Exit(0);
        }

        [EndpointMethod("has_dependencies")]
        static void HasDepedencies(HttpListenerContext ctx)
        {
            if (ctx.Request.Headers["Content-Type"] == "application/json")
            {
                using (var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    // Read the JSON body
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd());
                    DeviceInformation device;

                    // Find the matching device udid
                    lock (Devices)
                        device = Devices.FirstOrDefault(d => d.UDID == (string)data.udid);

                    // Check if we already have the dependencies
                    if (device == null)
                    {
                        SetResponse(ctx, new { error = "Unable to find the specified device. Are you sure it is connected?" });
                    }
                    else
                    {
                        // Obtain the status of the depedencies
                        var hasDeps = DeveloperImageHelper.HasImageForDevice(device);
                        var verStr = DeveloperImageHelper.GetSoftwareVersion(device);

                        // Automatically start download if it's missing
                        if (!hasDeps)
                        {
                            var links = DeveloperImageHelper.GetLinksForDevice(device);
                            if (links != null)
                            {
                                var state = new DownloadState(links.Select(t => t.Item1).ToArray(), links.Select(t => t.Item2).ToArray());
                                lock (Downloads)
                                    if (!Downloads.ContainsKey(verStr))
                                        Downloads[verStr] = state;
                                state.Start();
                            }
                            else
                            {
                                SetResponse(ctx, new { error = "Your device's iOS version is not supported at this time." });
                                return;
                            }
                        }

                        SetResponse(ctx, new { result = hasDeps, version = verStr });
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            // Configure paths
            string basePath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Environment.CurrentDirectory = basePath;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                NativeLibraries.Load();
            }
            catch
            {
                Console.WriteLine("Failed to load necessary files to run iFakeLocation.");
                return;
            }

            // Retrieve all web endpoint methods
            var methods =
                typeof(Program).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Select(mi => new Tuple<MethodInfo, object>(mi, mi.GetCustomAttributes(true).FirstOrDefault(ci => ci is EndpointMethod)))
                .Where(kvp => kvp.Item2 != null)
                .ToDictionary(kvp => ((EndpointMethod)kvp.Item2).Name, kvp => kvp.Item1);

            // Find a free port to run our local server on
            HttpListener listener;
            int port;
            if (!TryBindListenerOnFreePort(out listener, out port))
            {
                Console.WriteLine("Failed to initialise iFakeLocation (no free ports on local system).");
                return;
            }

            // Start window
            try
            {
                OpenBrowser($"http://localhost:{port}/");
                Console.WriteLine("iFakeLocation is now running at: " + $"http://localhost:{port}/");
                Console.WriteLine("\nPress Ctrl-C to quit (or click the close button).");
            }
            catch
            {
                Console.WriteLine("Unable to start iFakeLocation using default web browser.");
                return;
            }

            // Main processing loop
            while (true)
            {
                var ctx = listener.GetContext();
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    // Extract the method name from the URL
                    var methodName = ctx.Request.Url.Segments.Length > 1 ? string.Join("", ctx.Request.Url.Segments.Skip(1)) : "";
                    if (string.IsNullOrEmpty(methodName))
                        methodName = "main.html";

                    // Respond with static resource if specified
                    string path;
                    if (File.Exists(path = Path.Combine("Resources", methodName.Replace('/', Path.DirectorySeparatorChar))))
                    {
                        ctx.Response.Headers["Content-Type"] = MimeTypes.GetMimeType(methodName);
                        using (var s = File.OpenRead(path))
                            s.CopyTo(ctx.Response.OutputStream);
                        ctx.Response.Close();
                        return;
                    }
                    // Response with response from web method
                    else if (methods.TryGetValue(methodName, out MethodInfo method))
                    {
                        try
                        {
                            method.Invoke(null, new object[] { ctx });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("\n" + e);
                        }

                        try
                        {
                            if (ctx.Response.OutputStream.CanWrite)
                                ctx.Response.OutputStream.Close();
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                    else
                    {
                        // Response with nothing
                        ctx.Response.Close();
                        return;
                    }
                });
            }
        }
    }
}
