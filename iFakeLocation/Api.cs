using System.Globalization;
using System.Reflection;

namespace iFakeLocation;

public static class Api
{
    private static DeviceInformation[]? _devices = Array.Empty<DeviceInformation>();
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapGet("/", http =>
        {
            http.Response.Redirect("/index.html", permanent: true);
            return Task.CompletedTask;
        });

        app.UseStaticFiles();

        app.MapGet("version", () =>
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version!;
            return $"{v.Major}.{v.Minor}";
        });

        app.MapGet("home_country", () => RegionInfo.CurrentRegion.EnglishName);

        app.MapGet("get_devices", new Func<object>(() =>
        {
            // Save current devices
            try
            {
                if (_devices != null)
                    lock (_devices)
                        _devices = DeviceInformation.GetDevices();
            }
            catch (Exception e)
            {
                return new
                {
                    error = e.Message
                };
            }

            // No devices could be read, sent error
            if (_devices == null)
            {
                return new
                {
                    error =
                        "Unable to retrieve connected devices. Ensure iTunes is installed and can detect your device(s)."
                };
            }

            // Write devices to output
            return _devices.Select(d => new
            {
                name = d.DeviceName,
                display_name = d.ToString(),
                udid = d.Identifier
            });
        }));

        app.MapPost("stop_location", new Func<LocationRequest, object>(data =>
        {
            DeviceInformation? device = null;
            if (_devices != null)
                lock (_devices)
                    device = _devices.FirstOrDefault(d => d.Identifier == data.udid);

            // Check if we already have the dependencies
            if (device == null)
            {
                return new { error = "Unable to find the specified device. Are you sure it is connected?" };
            }

            try
            {
                device.StopLocation();
                return new { success = true };
            }
            catch (Exception e)
            {
                return new { error = e.Message };
            }
        }));

        app.MapPost("set_location", new Func<LocationRequest, Task<object>>(async data =>
        {
            DeviceInformation? device = null;

            // Find the matching device udid
            if (_devices != null)
                lock (_devices)
                    device = _devices.FirstOrDefault(d => d.Identifier == data.udid);

            if (device == null)
            {
                return new { error = "Unable to find the specified device. Are you sure it is connected?" };
            }

            try
            {
                await device.SetLocation(new Records { Lat = data.lat, Lng = data.lng });
                return new { success = true };
            }
            catch (TimeoutException)
            {
                return new { success = true };
            }
            catch (Exception e)
            {
                return new { error = e.Message };
            }
        }));

        app.MapGet("exit", () => Environment.Exit(0));
    }
}