namespace Shoutr.WebServer.Hubs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubs(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IFrontEndHub,FrontEndHub>();
    }
}