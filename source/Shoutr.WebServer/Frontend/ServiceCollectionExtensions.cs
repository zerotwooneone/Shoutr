using Shoutr.WebServer.Hubs;

namespace Shoutr.WebServer.Frontend;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFrontend(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<FrontendService>();
    }
}