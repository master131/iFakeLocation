using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace iFakeLocation;

public class StartupHostedService(IServer server) : IHostedService
{
    private readonly IServerAddressesFeature? _serverAddresses = server.Features.Get<IServerAddressesFeature>();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        PyMobileDevice.StartTunnel();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        PyMobileDevice.StopTunnel();
        return Task.CompletedTask;
    }
}