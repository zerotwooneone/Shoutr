namespace Shoutr.WebServer.Reactive;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReactive(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<SchedulerProvider>();
    }
}