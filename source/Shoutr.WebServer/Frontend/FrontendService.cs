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
    private readonly FrontEndHub _frontEndHub;
    private readonly SchedulerProvider _schedulerProvider;
    private CompositeDisposable _disposable = new();
    private readonly Subject<Unit> _cancelFake = new();

    public FrontendService(
        FrontEndHub frontEndHub,
        SchedulerProvider schedulerProvider)
    {
        _frontEndHub = frontEndHub;
        _schedulerProvider = schedulerProvider;
        _disposable.Add(_frontEndHub.Connected.Subscribe(OnConnected));
        _disposable.Add(_frontEndHub.Disconnected.Subscribe(OnDisconnected));
    }

    private void OnConnected(string connectionId)
    {
        //cancel and fake data now
        _cancelFake.OnNext();

        Observable.Delay(Observable.Empty<HubPeer>(), TimeSpan.FromSeconds(3))
            .Concat(Observable.Return(new HubPeer
            {
                Id = "First hub peer",
                Nickname = "First Nickname",
            }).Delay(TimeSpan.FromMilliseconds(300)))
            .Concat(Observable.Return(new HubPeer
            {
                Id = "Second hub peer",
                Nickname = "Second Nickname",
            }).Delay(TimeSpan.FromSeconds(3)))
            .Concat(Observable.Return(new HubPeer
            {
                Id = "Third hub peer",
                Nickname = "Third Nickname",
            }).Delay(TimeSpan.FromSeconds(3)))
            .TakeUntil(_cancelFake.AsObservable())
            .SubscribeAsync(async p =>
                {
                    await _frontEndHub.SendPeerChanged(p);
                },
                ex =>
                {
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
}