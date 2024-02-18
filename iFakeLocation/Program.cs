using iFakeLocation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<StartupHostedService>();
var app = builder.Build();

app.MapEndpoints();

if (!Helpers.IsUserAdministrator())
{
    Console.WriteLine("Please run this program as an administrator.");
    return -1;
}

try
{
    await PyMobileDevice.SetupPython();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine("Failed to load necessary files to run iFakeLocation.");
    return -1;
}

try
{
    await app.StartAsync();
    Helpers.OpenBrowser(app.Urls.First());
    await app.WaitForShutdownAsync();
    return 0;
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
finally
{
    PyMobileDevice.ShutdownPython();
}
