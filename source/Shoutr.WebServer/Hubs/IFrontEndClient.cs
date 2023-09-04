namespace Shoutr.WebServer.Hubs;

public interface IFrontEndClient
{
    Task PeerChanged(HubPeer peer);
}