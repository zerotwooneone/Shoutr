using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using Shoutr.Contracts;
using Shoutr.Serialization;

namespace Shoutr
{
    public class Listener : IListener
    {
        public async Task Listen(int port = Defaults.Port,
            CancellationToken cancellationToken = default)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = cancellationTokenSource.Token;

            var packetBufferObservable = new Subject<byte[]>();
            var observableScheduler =
                System.Reactive.Concurrency.Scheduler.Default; //new TaskPoolScheduler(new TaskFactory(token));
            var protoMessageObservable = packetBufferObservable
                .ObserveOn(observableScheduler)
                .Select(bytes =>
                {
                    ProtoMessage message;
                    using (var memoryStream = new MemoryStream(bytes))
                    {
                        var protoMessage = Serializer.Deserialize<ProtoMessage>(memoryStream);
                        message = protoMessage;
                    }

                    //System.Console.WriteLine(
                    //    $"id:{new Guid(message.BroadcastId)} {JsonConvert.SerializeObject(message)}");
                    return message;
                });

            var headerCache = new ConcurrentDictionary<Guid, Header>();
            var payloadCache = new ConcurrentDictionary<Guid, List<ProtoMessage>>();

            var fileWriteRequestSubject = new Subject<FileWriteWrapper>();

            var headerObservable = protoMessageObservable
                .Where(message => !string.IsNullOrWhiteSpace(message.FileName) && message.PayloadCount.HasValue);

            var payloadObservable = protoMessageObservable
                .Where(message => message.PayloadIndex.HasValue && message.Payload != null);

            string GetFileName(ProtoMessage header)
            {
                return $"{System.DateTime.Now:HHmmssffff}.{header.FileName}";
            }

            Header ConvertToHeader(ProtoMessage header)
            {
                return new Header
                {
                    BroadcastId = header.GetBroadcastId(), FileName = GetFileName(header),
                    PayloadCount = header.PayloadCount.Value, PayloadMaxBytes = header.PayloadMaxSize.Value
                };
            }

            headerObservable
                .ObserveOn(observableScheduler)
                .Subscribe(header => headerCache.AddOrUpdate(header.GetBroadcastId(),
                    bcid =>
                    {
                        if (payloadCache.TryRemove(bcid, out var payloads))
                        {
                            foreach (var payload in payloads)
                            {
                                fileWriteRequestSubject.OnNext(new FileWriteWrapper()
                                    {Header = ConvertToHeader(header), Payload = payload});
                            }
                        }

                        return ConvertToHeader(header);
                    },
                    (bcid, m) => m));


            payloadObservable
                .ObserveOn(observableScheduler)
                .Subscribe(payload =>
                {
                    if (headerCache.TryGetValue(payload.GetBroadcastId(), out var header))
                    {
                        fileWriteRequestSubject.OnNext(new FileWriteWrapper() {Header = header, Payload = payload});
                    }
                    else
                    {
                        payloadCache.AddOrUpdate(payload.GetBroadcastId(),
                            bcid =>
                            {
                                var list = new List<ProtoMessage>();
                                list.Add(payload);
                                return list;
                            },
                            (bcid, list) =>
                            {
                                list.Add(payload);
                                return list;
                            });
                    }
                });

            var writeCompleteObservable = fileWriteRequestSubject
                //.ObserveOn(observableScheduler)
                .Select(writeRequest =>
                {
                    return Observable.FromAsync(async () =>
                    {
                        var file = new FileInfo(writeRequest.Header.FileName);
                        using (var stream = file.OpenWrite())
                        {
                            var writeIndex = writeRequest.Payload.PayloadIndex.Value *
                                             writeRequest.Header.PayloadMaxBytes;
                            stream.Seek(writeIndex, SeekOrigin.Begin);
                            await stream.WriteAsync(writeRequest.Payload.Payload, token).ConfigureAwait(false);
                        }

                        return writeRequest.Header;
                    });
                }).Merge(1);

            var fileWriteObservable = writeCompleteObservable
                .ObserveOn(observableScheduler)
                .GroupBy(w => w.BroadcastId);

            var fileTimeout = TimeSpan.FromSeconds(10);
            Func<IGroupedObservable<Guid, Header>, IObservable<Header>> createAmb = null;

            IObservable<Header> CreateAmb(IGroupedObservable<Guid, Header> g)
            {
                var nextAmb = g.FirstOrDefaultAsync().Select(x => createAmb(g)).Switch();
                var timeoutObservable = Observable.Return(g).Merge().FirstOrDefaultAsync().Delay(fileTimeout);
                return Observable.Amb(nextAmb, timeoutObservable);
            }

            createAmb = CreateAmb;

            var fileStoppedObservable = fileWriteObservable
                .SelectMany(g => CreateAmb(g))
                .Select(p =>
                {
                    OnBroadcastEnded(new BroadcastResult(p.BroadcastId, p.FileName));
                    return p.FileName;
                });

            var fileStoppedSub = fileStoppedObservable.Subscribe();
            token.Register(() => fileStoppedSub.Dispose());

            UdpClient receiver = new UdpClient(port);
            //receiver.EnableBroadcast = true;
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0); //new IPEndPoint(IPAddress.Parse("192.168.1.255"), 0);
            while (!token.IsCancellationRequested)
            {
                var received = await receiver.ReceiveAsync().ConfigureAwait(false);
                //System.Console.WriteLine($"Buff bytes {received.Buffer.Length}");
                packetBufferObservable.OnNext(received.Buffer);
            }
        }

        public event EventHandler<IBroadcastResult> BroadcastEnded;

        protected virtual void OnBroadcastEnded(IBroadcastResult e)
        {
            BroadcastEnded?.Invoke(this, e);
        }
    }
}