using Shoutr.WebServer.Hubs;

namespace Shoutr.WebServer.Frontend;

public class FrontendService
{
    private readonly FrontEndHub _frontEndHub;

    public FrontendService(FrontEndHub frontEndHub)
    {
        _frontEndHub = frontEndHub;
    }
}