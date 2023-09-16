using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Shoutr.WebServer.Hubs;
using Shoutr.WebServer.Reactive;

namespace Shoutr.WebServer.Frontend;

public class FrontendService
{
    private readonly IFrontEndHub _frontEndHub;
    private readonly SchedulerProvider _schedulerProvider;
    private CompositeDisposable _disposable = new();
    private readonly Subject<Unit> _cancelFake = new();

    public FrontendService(
        IFrontEndHub frontEndHub,
        SchedulerProvider schedulerProvider)
    {
        _frontEndHub = frontEndHub;
        _schedulerProvider = schedulerProvider;
        _disposable.Add(_frontEndHub.Connected.SubscribeAsync(OnConnected));
        _disposable.Add(_frontEndHub.Disconnected.Subscribe(OnDisconnected));
        _disposable.Add(_frontEndHub.DownloadRequest.Subscribe(OnDownloadRequest));
        _disposable.Add(_frontEndHub.CancelRequest.Subscribe(OnCancelRequest));
    }

    private async Task OnConnected(string connectionId)
    {
        //cancel and fake data now
        _cancelFake.OnNext();

        await _frontEndHub.SendConfig(connectionId,
            new HubConfig("backend user id", "backend public key slkdfjsldkfj fdjskfjsd;kjfasj sdfjsdflkdfs jd"));

        Observable.Empty<HubBroadcast>().Delay(TimeSpan.FromSeconds(3))
            .Concat(Observable.Return(new HubBroadcast("First backend broadcast")))
            .Concat(Observable.Return(new HubBroadcast("Second backend broadcast")).Delay(TimeSpan.FromSeconds(1.3)))
            .Concat(Observable.Return(new HubBroadcast("Third backend broadcast")).Delay(TimeSpan.FromSeconds(0.9)))
            .Concat(Observable.Range(0,101)
                .Select(pct => 
                    Observable.Return(new HubBroadcast("Second backend broadcast"){ PercentComplete = pct})
                        .Delay(TimeSpan.FromSeconds(0.3)))
                .Concat())
            .Concat(Observable.Return(new HubBroadcast("Second backend broadcast"){ Completed = true}).Delay(TimeSpan.FromSeconds(0.3)))
            .Concat(Observable.Return(new HubBroadcast("Third backend broadcast"){ Completed = true}).Delay(TimeSpan.FromSeconds(1.3)))
            .TakeUntil(_cancelFake.AsObservable())
            .SubscribeAsync(async hb =>
                {
                    await _frontEndHub.SendBroadcastChanged(hb);
                },
                ex =>
                {
                    //todo: log this
                    int x = 0;
                });
        
        Observable.Empty<HubPeer>().Delay(TimeSpan.FromSeconds(3))
            .Concat(Observable.Return(new HubPeer("First backend peer","First backend Nickname")).Delay(TimeSpan.FromMilliseconds(300)))
            .Concat(Observable.Return(new HubPeer("Second backend peer","Second backend Nickname")).Delay(TimeSpan.FromSeconds(3)))
            .Concat(Observable.Return(new HubPeer("Third backend peer","Third backend Nickname")).Delay(TimeSpan.FromSeconds(3)))
            .TakeUntil(_cancelFake.AsObservable())
            .SubscribeAsync(async p =>
                {
                    await _frontEndHub.SendPeerChanged(p);
                },
                ex =>
                {
                    //todo: log this
                    int x = 0;
                });
    }

    private IScheduler GetScheduler()
    {
        return _schedulerProvider.Get("Frontend service");
    }

    private void OnDisconnected(string connectionId)
    {
        
    }

    private void OnDownloadRequest(string broadcastId)
    {
        Observable.Return(new HubBroadcast(broadcastId))
            .Concat(Observable.Range(0,101)
                .Select(pct => 
                    Observable.Return(new HubBroadcast(broadcastId){ PercentComplete = pct})
                        .Delay(TimeSpan.FromSeconds(0.3)))
                .Concat())
            .Concat(Observable.Return(new HubBroadcast(broadcastId){ Completed = true}).Delay(TimeSpan.FromSeconds(0.3)))
            .TakeUntil(_cancelFake)
            .SubscribeAsync( async p =>
                {
                    await _frontEndHub.SendBroadcastChanged(p);
                },
                ex =>
                {
                    //todo: log this
                    int x = 0;
                });
    }
    
    private void OnCancelRequest(string broadcastId)
    {
        _cancelFake.OnNext();
    }
}