using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.SignalR;
using Shoutr.WebServer.Reactive;

namespace Shoutr.WebServer.Hubs;

public class FrontEndHub: Hub<IFrontEndClient>
{
    private readonly SchedulerProvider _schedulerProvider;

    private readonly Subject<string> _connected = new();
    public IObservable<string> Connected => AsObservable(_connected);

    private readonly Subject<string> _disconnected = new();
    public IObservable<string> Disconnected => AsObservable(_disconnected);

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
}