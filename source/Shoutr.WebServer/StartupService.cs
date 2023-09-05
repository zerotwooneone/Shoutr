using Shoutr.WebServer.Frontend;

public class StartupService: IHostedService
{
    public StartupService(FrontendService frontendService)
    {
        //if the service subscribes in the constructor, then nothing more is needed
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //intentionally empty
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //intentionally empty
    }
}