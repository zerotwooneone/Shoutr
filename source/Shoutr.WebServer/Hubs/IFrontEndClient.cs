namespace Shoutr.WebServer.Hubs;

/// <summary>
/// The methods which can be executed on any and all clients
/// </summary>
/// <remarks>There is no C# implementation of this interface. There is no instance that can be injected or called, you must use the <see cref="IFrontEndHub"/> to call these methods. </remarks>
public interface IFrontEndClient
{
    Task PeerChanged(HubPeer peer);
    Task BroadcastChanged(HubBroadcast broadcast);
    Task SendConfigToClient(HubConfig config);
}