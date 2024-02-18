using System.Diagnostics;
using System.Text.RegularExpressions;
using Python.Included;
using Python.Runtime;
using Timer = System.Timers.Timer;

namespace iFakeLocation;

public static partial class PyMobileDevice
{
    private static Process? _tunnelProcess;
    private static Process? _setLocationProcess;

    public static async Task SetupPython()
    {
        await Installer.SetupPython();
        await Installer.TryInstallPip();
        await Installer.PipInstallModule("pymobiledevice3");
        PythonEngine.Initialize();
    }

    public static void ShutdownPython()
    {
        StopTunnel();
        PythonEngine.Shutdown();
    }

    public static void StartTunnel()
    {
        if (_tunnelProcess is { HasExited: false }) return;

        _tunnelProcess = RunPyMobileDevice("remote tunneld", false);

        if (RunPyMobileDevice("mounter auto-mount", true).ExitCode == 0) return;

        Console.WriteLine("Failed to mount developer image.");
        StopTunnel();
        throw new Exception("Failed to mount developer image. Is the device locked?");
    }

    internal static void StopTunnel()
    {
        _tunnelProcess?.Kill();
    }

    internal static string GetDevices()
    {
        var process = RunPyMobileDevice("usbmux list", true);
        return CleanStdOutput(process.StandardOutput.ReadToEnd());
    }

    internal static void ClearLocation(DeviceInformation device)
    {
        StartTunnel();
        _setLocationProcess?.Kill();
        RunPyMobileDevice(
            device.IsIos17OrHigher
                ? $"developer dvt simulate-location clear --tunnel {device.Identifier}"
                : $"developer simulate-location clear --tunnel {device.Identifier}", true);
    }

    internal static async Task SetLocation(DeviceInformation device, string location)
    {
        StartTunnel();
        _setLocationProcess?.Kill();
        _setLocationProcess = RunPyMobileDevice(
            device.IsIos17OrHigher
                ? $"developer dvt simulate-location set --tunnel {device.Identifier} -- {location}"
                : $"developer simulate-location set --tunnel {device.Identifier} -- {location}", false);

        string error;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        do
        {
            if (stopwatch.ElapsedMilliseconds > 3000)
            {
                throw new TimeoutException("Location setting is waiting for user input.");
            }
            await Task.Delay(1000);
            error = await _setLocationProcess.StandardError.ReadLineAsync() ?? "";
            if (!error.Contains("ERROR")) continue;

            var message = CleanErrorOutput(error);
            throw new Exception($"{message}. Try disconnecting and reconnecting the device.");
        } while (!string.IsNullOrEmpty(error));
    }

    private static string CleanStdOutput(string input)
    {
        var groups = StdOutputCleanerRegex().Matches(input).Select(x => x.Groups[1].Value).Select(Regex.Unescape);
        return string.Join("", groups);
    }

    private static string CleanErrorOutput(string input)
    {
        var groups = ErrorOutputCleanerRegex().Matches(input).Select(x => x.Groups[1].Value).Select(Regex.Unescape);
        return string.Join("", groups);
    }


    private static Process RunPyMobileDevice(string arguments, bool waitForExit)
    {
        var process = new Process();
        process.StartInfo.FileName = "python";
        process.StartInfo.Arguments = $"-m pymobiledevice3 {arguments}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        if (waitForExit)
        {
            process.WaitForExit();
        }
        return process;
    }

    [GeneratedRegex(@"\u001b\[38;2;\d+;\d+;\d+m(.*?)\u001b\[39m")]
    private static partial Regex StdOutputCleanerRegex();

    [GeneratedRegex(@"ERROR\u001b\[0m\ \u001b\[\d+m(.*?)\u001b\[0m")]
    private static partial Regex ErrorOutputCleanerRegex();
}