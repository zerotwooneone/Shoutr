namespace Shoutr.WebServer.Hubs;

public interface IFrontEndClient
{
    Task PeerChanged(HubPeer peer);
    Task BroadcastChanged(HubBroadcast broadcast);
    Task SendConfigToClient(HubConfig config);
}