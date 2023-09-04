using Microsoft.AspNetCore.SignalR;

namespace Shoutr.WebServer.Hubs;

public class FrontEndHub: Hub<IFrontEndClient>
{
    public async Task SendPeerChanged(HubPeer peer)
    {
        await Clients.All.PeerChanged(peer);
    }
}