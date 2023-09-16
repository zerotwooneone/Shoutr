using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.SignalR;
using Shoutr.WebServer.Reactive;

namespace Shoutr.WebServer.Hubs;

public class FrontEndHub: Hub<IFrontEndClient>, IFrontEndHub
{
    private readonly SchedulerProvider _schedulerProvider;

    private readonly Subject<string> _connected = new();
    public IObservable<string> Connected => AsObservable(_connected);

    private readonly Subject<string> _disconnected = new();
    public IObservable<string> Disconnected => AsObservable(_disconnected);
    private readonly Subject<string> _downloadRequest = new();
    public IObservable<string> DownloadRequest => AsObservable(_downloadRequest);
    
    private readonly Subject<string> _cancelRequest = new();
    public IObservable<string> CancelRequest => AsObservable(_cancelRequest);

    public FrontEndHub(SchedulerProvider schedulerProvider)
    {
        _schedulerProvider = schedulerProvider;
    }

    public async Task SendPeerChanged(HubPeer peer)
    {
        await Clients.All.PeerChanged(peer);
    }

    public override async Task OnConnectedAsync()
    {
        _connected.OnNext(Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _disconnected.OnNext(Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    private IObservable<T> AsObservable<T>(Subject<T> subject)
    {
        return subject.AsObservable().ObserveOn(_schedulerProvider.Get("Hub"));
    }

    public async Task SendBroadcastChanged(HubBroadcast hubBroadcast)
    {
        await Clients.All.BroadcastChanged(hubBroadcast);
    }

    public async Task SendConfig(string connectionId, HubConfig hubConfig)
    {
        await Clients.Client(connectionId).SendConfigToClient(hubConfig);
    }

    public async Task<bool> Download(string broadcastId)
    {
        _downloadRequest.OnNext(broadcastId);
        return true;
    }

    public async Task<bool> UserCancel(string broadcastId)
    {
        _cancelRequest.OnNext(broadcastId);
        return true;
    }
}