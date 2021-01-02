using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using Shoutr.Contracts;
using Shoutr.Contracts.ByteTransport;
using Shoutr.Reactive;
using Shoutr.Serialization;

namespace Shoutr
{
    public class Listener : IListener
    {
        public async Task Listen(IByteReceiver byteReceiver,
            CancellationToken cancellationToken = default)
        {
            var observableScheduler =
                System.Reactive.Concurrency.Scheduler.Default; //new TaskPoolScheduler(new TaskFactory(token)); 
            await Listen(byteReceiver,
                observableScheduler,
                cancellationToken).ConfigureAwait(false);
        }
        
        public async Task Listen(IByteReceiver byteReceiver,
            IScheduler scheduler,
            CancellationToken cancellationToken = default)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = cancellationTokenSource.Token;

            var packetBufferObservable = new Subject<byte[]>();
            var protoMessageObservable = packetBufferObservable
                .ObserveOn(scheduler)
                .Select(bytes =>
                {
                    ProtoMessage message;
                    using (var memoryStream = new MemoryStream(bytes))
                    {
                        var protoMessage = Serializer.Deserialize<ProtoMessage>(memoryStream);
                        message = protoMessage;
                    }

                    //DdsLog($"deserialized \r\n {Newtonsoft.Json.JsonConvert.SerializeObject(new {bcid= message.GetBroadcastId(), message.PayloadCount, message.PayloadIndex, payloadLength=message.Payload?.Length})}", true);
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
                .ObserveOn(scheduler)
                .Subscribe(header => headerCache.AddOrUpdate(header.GetBroadcastId(),
                    bcid =>
                    {
                        DdsLog($"header addOrUpdate {bcid}");
                        if (payloadCache.TryRemove(bcid, out var payloads))
                        {
                            var p = payloads.ToArray(); //todo:figure out why this bombed - List changed exception
                            payloads.Clear();
                            DdsLog($"empty cache {p.Length}");
                            foreach (var payload in p)
                            {
                                DdsLog($"cached write request {payload.PayloadIndex}");
                                fileWriteRequestSubject.OnNext(new FileWriteWrapper()
                                    {Header = ConvertToHeader(header), Payload = payload});
                            }
                        }

                        return ConvertToHeader(header);
                    },
                    (bcid, m) => m)
                );


            payloadObservable
                .ObserveOn(scheduler)
                .Subscribe(payload =>
                {
                    if (headerCache.TryGetValue(payload.GetBroadcastId(), out var header))
                    {
                        DdsLog($"write request {payload.PayloadIndex}");
                        fileWriteRequestSubject.OnNext(new FileWriteWrapper() {Header = header, Payload = payload});
                    }
                    else
                    {
                        DdsLog($"write cached {payload.PayloadIndex}");
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
                        await using (var stream = file.OpenWrite())
                        {
                            var writeIndex = writeRequest.Payload.PayloadIndex.Value *
                                             writeRequest.Header.PayloadMaxBytes;
                            stream.Seek(writeIndex, SeekOrigin.Begin);
                            await stream.WriteAsync(writeRequest.Payload.Payload, token).ConfigureAwait(false);
                        }
                        DdsLog($"write complete {writeRequest.Payload.PayloadIndex}");
                        return writeRequest.Header;
                    });
                }).Merge(1);

            var fileWriteObservable = writeCompleteObservable
                .ObserveOn(scheduler)
                .GroupBy(w => w.BroadcastId);

            const int fileCompleteTimeout = 10;
            var fileTimeout = TimeSpan.FromSeconds(fileCompleteTimeout);
            var fileStoppedObservable = CreateTimeoutObservable(fileTimeout, fileWriteObservable, scheduler);

            var fileStoppedSub = fileStoppedObservable.Subscribe(header =>
            {
                OnBroadcastEnded(new BroadcastResult(header.BroadcastId, header.FileName));
            });
            token.Register(() => fileStoppedSub.Dispose());

            void OnBytesReceived(object sender, IBytesReceived bytesReceived)
            {
//DdsLog($"bytes received {bytesReceived.Bytes.Length}");
                packetBufferObservable.OnNext(bytesReceived.Bytes);
            }
            byteReceiver.BytesReceived += OnBytesReceived;
            cancellationToken.Register(() =>
            {
                byteReceiver.BytesReceived -= OnBytesReceived;
            });
            await byteReceiver.Listen(cancellationToken);
        }

        private IObservable<Header> CreateTimeoutObservable(TimeSpan completeTimeout,
            IObservable<IGroupedObservable<Guid, Header>> observable,
            IScheduler scheduler)
        {
            var fileStoppedObservable = observable
                .SelectMany(fileWriteObservable =>
                {
                    return Observable.FromAsync(async () =>
                    {
                        var first = await fileWriteObservable.FirstOrDefaultAsync();
                        if (first == null)
                        {
                            return Observable.Empty<Header>();
                        }

                        return fileWriteObservable.WhenStopped((Header) first, completeTimeout, scheduler);
                    });
                })
                .Merge();

            return fileStoppedObservable;
        }

        internal static void DdsLog(string message, 
            bool includeDetails = false,
            [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            if (includeDetails)
            {
                Console.Write($"{caller} thread:{System.Threading.Thread.CurrentThread.ManagedThreadId} ");    
            }
            Console.WriteLine($"{message}");
        }

        public event EventHandler<IBroadcastResult> BroadcastEnded;

        protected virtual void OnBroadcastEnded(IBroadcastResult e)
        {
            BroadcastEnded?.Invoke(this, e);
        }
    }
}
