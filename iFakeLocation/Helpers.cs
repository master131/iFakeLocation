using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace iFakeLocation;

internal static class Helpers
{
    internal static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
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
            // throw
        }
    }

    internal static bool IsUserAdministrator()
    {
        bool isAdmin;
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                isAdmin = getuid() == 0;
            }
            else
            {
                isAdmin = false;
            }
        }
        catch (Exception)
        {
            isAdmin = false;
        }
        return isAdmin;
    }

    [DllImport("libc")]
    private static extern uint getuid();
}