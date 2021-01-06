using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using Shoutr.Contracts;
using Shoutr.Contracts.ByteTransport;
using Shoutr.Contracts.Io;
using Shoutr.Reactive;
using Shoutr.Serialization;

namespace Shoutr
{
    public class Listener : IListener
    {
        public async Task Listen(IByteReceiver byteReceiver,
            IStreamFactory streamFactory,
            CancellationToken cancellationToken = default)
        {
            var observableScheduler =
                Scheduler.Default; //new TaskPoolScheduler(new TaskFactory(token)); 
            await Listen(byteReceiver,
                streamFactory,
                observableScheduler,
                cancellationToken).ConfigureAwait(false);
        }

        public async Task Listen(IByteReceiver byteReceiver,
            IStreamFactory streamFactory,
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
                return $"{System.DateTime.Now:HH-mm-ss-ffff}.{header.FileName}";
            }

            Header ConvertToHeader(ProtoMessage header)
            {
                return new Header
                {
                    BroadcastId = header.GetBroadcastId(), FileName = GetFileName(header),
                    PayloadCount = header.PayloadCount.Value, PayloadMaxBytes = header.PayloadMaxSize.Value
                };
            }

            void HandleCachedPayloads(Guid guid, Header header1)
            {
                if (payloadCache.TryRemove(guid, out var payloads))
                {
                    var p = payloads.ToArray(); //todo:figure out why this bombed - List changed exception
                    payloads.Clear();
                    foreach (var payload in p)
                    {
                        fileWriteRequestSubject.OnNext(new FileWriteWrapper()
                            {Header = header1, Payload = payload});
                    }
                }
            }

            headerObservable
                .ObserveOn(scheduler)
                .Subscribe(protoHeader => headerCache.AddOrUpdate(protoHeader.GetBroadcastId(),
                    bcid =>
                    {
                        //todo: decide if header.payloadCount X headerMaxPayloadSize is too much
                        var header = ConvertToHeader(protoHeader);
                        HandleCachedPayloads(bcid, header);

                        return header;
                    },
                    (bcid, header) =>
                    {
                        HandleCachedPayloads(bcid, header);
                        return header;
                    })
                );

            var md5 = MD5.Create();

            bool CheckHash(ProtoMessage payload)
            {
                var expected = payload.Hash;
                if (expected == null || expected.Length == 0)
                {
                    return false;
                }

                var actual = md5.ComputeHash(payload.Payload);
                if (actual.Length == 0 || expected.Length != actual.Length)
                {
                    return false;
                }

                for (var index = 0; index < expected.Length; index++)
                {
                    var expectedByte = expected[index];
                    var actualByte = actual[index];
                    if (expectedByte != actualByte)
                    {
                        return false;
                    }
                }

                return true;
            }

            payloadObservable
                .ObserveOn(scheduler)
                .Subscribe(payload =>
                {
                    if (!CheckHash(payload))
                    {
                        throw new InvalidDataException("Hash check failed");
                        //todo: return error statistics to the caller
                    }
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
                        using var writer = streamFactory.CreateWriter(writeRequest.Header.FileName);
                        var writeIndex = writeRequest.Payload.PayloadIndex.Value *
                                         writeRequest.Header.PayloadMaxBytes;
                        await writer.Write(writeIndex, writeRequest.Payload.Payload, token).ConfigureAwait(false);
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
                packetBufferObservable.OnNext(bytesReceived.Bytes);
            }

            byteReceiver.BytesReceived += OnBytesReceived;
            cancellationToken.Register(() => { byteReceiver.BytesReceived -= OnBytesReceived; });
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

        public event EventHandler<IBroadcastResult> BroadcastEnded;

        protected virtual void OnBroadcastEnded(IBroadcastResult e)
        {
            BroadcastEnded?.Invoke(this, e);
        }
    }
}
