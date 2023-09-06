using Shoutr.WebServer.Frontend;

public class StartupService: IHostedService
{
    public StartupService(FrontendService frontendService)
    {
        //if the service subscribes in the constructor, then nothing more is needed
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        //intentionally empty
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        //intentionally empty
        return Task.CompletedTask;
    }
}