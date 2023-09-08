namespace Shoutr.WebServer.Hubs;

public interface IFrontEndHub
{
    /// <summary>
    /// Tell all clients that a broadcast has changed
    /// </summary>
    /// <param name="hubBroadcast"></param>
    /// <returns></returns>
    Task SendBroadcastChanged(HubBroadcast hubBroadcast);
    /// <summary>
    /// Tell all clients that a peer has changed
    /// </summary>
    /// <param name="peer"></param>
    /// <returns></returns>
    Task SendPeerChanged(HubPeer peer);
    /// <summary>
    /// Send the target client this hubs config
    /// </summary>
    /// <param name="connectionId">the target client</param>
    /// <param name="hubConfig"></param>
    /// <returns></returns>
    Task SendConfig(string connectionId, HubConfig hubConfig);
    /// <summary>
    /// Contains the broadcast id when a client wants to cancel a download
    /// </summary>
    IObservable<string> CancelRequest { get; }
    /// <summary>
    /// Contains the broadcast id when a client wants to download
    /// </summary>
    IObservable<string> DownloadRequest { get; }
    /// <summary>
    /// Contains the connection id when a client connects
    /// </summary>
    IObservable<string> Connected { get; }
    /// <summary>
    /// Contains the connection id when a client disconnects
    /// </summary>
    IObservable<string> Disconnected { get; }
}