using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts;
using Shoutr.Contracts.ByteTransport;
using Shoutr.Contracts.Io;
using Shoutr.Reactive;
using Shoutr.Serialization;

namespace Shoutr
{
    public class Listener : IListener
    {
        private readonly ISchedulerLocator _schedulerLocator;

        public Listener(ISchedulerLocator schedulerLocator)
        {
            _schedulerLocator = schedulerLocator;
        }
        public async Task Listen(IByteReceiver byteReceiver,
            IStreamFactory streamFactory,
            string destinationPath = "",
            CancellationToken cancellationToken = default)
        {
            var basePath = string.IsNullOrWhiteSpace(destinationPath)
                ? ""
                : destinationPath;
            if (basePath != "")
            {
                Directory.CreateDirectory(basePath); //does not throw an exception if it does not exist    
            }

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = cancellationTokenSource.Token;

            var packetBufferObservable = new Subject<byte[]>();

            IObservable<ProtoMessage> GetProtoMessages()
            {
                return packetBufferObservable
                    .ObserveOn(_schedulerLocator.GetScheduler("listen packet"))
                    .Select(bytes =>
                    {
                        var message = ProtoMessage.Parser.ParseFrom(bytes);
                        return message;
                    });
            }

            var protoMessageObservable = GetProtoMessages()
                .Catch((Exception e) =>
                {
                    //todo:log these errors
                    return GetProtoMessages();
                });

            var headerCache = new ConcurrentDictionary<Guid, Header>();
            var payloadCache = new ConcurrentDictionary<Guid, List<ProtoMessage>>();

            var fileWriteRequestSubject = new Subject<FileWriteWrapper>();

            var headerObservable = protoMessageObservable
                .Where(message => !string.IsNullOrWhiteSpace(message.FileName) && message.HasPayloadCount);

            var payloadObservable = protoMessageObservable
                .Where(message => message.HasPayloadIndex && message.Payload != null);

            string GetFileName(ProtoMessage header)
            {
                return $"{System.DateTime.Now:HH-mm-ss-ffff}.{header.FileName}";
            }

            Header ConvertToHeader(ProtoMessage header)
            {
                var broadcastId = header.GetBroadcastId();
                if (broadcastId == null)
                {
                    throw new ArgumentException("broadcast id cannot be null", nameof(header));
                }
                return new Header
                {
                    BroadcastId = broadcastId.Value, 
                    FileName = GetFileName(header),
                    PayloadCount = header.PayloadCount, 
                    PayloadMaxBytes = header.PayloadMaxSize
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
                .ObserveOn(_schedulerLocator.GetScheduler("listen header"))
                .Subscribe(protoHeader => headerCache.AddOrUpdate(protoHeader.GetBroadcastId().Value,
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

                var actual = md5.ComputeHash(payload.Payload.ToByteArray());
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
                .ObserveOn(_schedulerLocator.GetScheduler("listen payload"))
                .Subscribe(payload =>
                {
                    if (!CheckHash(payload))
                    {
                        throw new InvalidDataException("Hash check failed");
                        //todo: return error statistics to the caller
                    }

                    var broadcastId = payload.GetBroadcastId().Value;
                    if (headerCache.TryGetValue(broadcastId, out var header))
                    {
                        fileWriteRequestSubject.OnNext(new FileWriteWrapper() {Header = header, Payload = payload});
                    }
                    else
                    {
                        payloadCache.AddOrUpdate(broadcastId,
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
                    if (writeRequest.Payload.Payload.Length > writeRequest.Header.PayloadMaxBytes)
                    {
                        throw new ArgumentException("Payload exceeds max byte length");
                    }
                    return Observable.FromAsync(async () =>
                    {
                        var fullPath = Path.Combine(basePath, writeRequest.Header.FileName);
                        using var writer = streamFactory.CreateWriter(fullPath);
                        var writeIndex = writeRequest.Payload.PayloadIndex *
                                         writeRequest.Header.PayloadMaxBytes;
                        await writer.Write(writeIndex, writeRequest.Payload.Payload.ToByteArray(), token).ConfigureAwait(false);
                        return writeRequest.Header;
                    });
                }).Merge(1);

            var fileWriteObservable = writeCompleteObservable
                .ObserveOn(_schedulerLocator.GetScheduler("listen write"))
                .GroupBy(w => w.BroadcastId);

            const int fileCompleteTimeout = 10;
            var fileTimeout = TimeSpan.FromSeconds(fileCompleteTimeout);
            var fileStoppedObservable = CreateTimeoutObservable(fileTimeout, fileWriteObservable);

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
            IObservable<IGroupedObservable<Guid, Header>> observable)
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

                        return fileWriteObservable.RepeatWhile((Header) first, completeTimeout, _schedulerLocator.GetScheduler("listen file stopped"));
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
