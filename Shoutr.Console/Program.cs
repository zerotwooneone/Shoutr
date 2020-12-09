using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ProtoBuf;

namespace Shoutr.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");

            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var tokenSource = new CancellationTokenSource();

            void OnCancelKey(object sender, ConsoleCancelEventArgs e)
            {
                tokenSource.Cancel();
            }

            System.Console.CancelKeyPress += OnCancelKey;
            var program = new Program();

            const string fileName = "test.7z";
            const int port = 3036;

            try
            {
                if (config["listen"] != null)
                {
                    System.Console.WriteLine("Going to listen");
                    program.Listen(port, tokenSource.Token, f => System.Console.WriteLine($"file complete: {f}"))
                        .Wait(tokenSource.Token);
                }
                else if (config["broadcast"] != null)
                {
                    System.Console.WriteLine("Going to broadcast");
                    program.Broadcast(fileName, port, tokenSource.Token)
                        .Wait(tokenSource.Token);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"fatal error: {e}");
            }
        }

        private Program()
        {
        }

        private async Task Broadcast(string fileName, int port, CancellationToken programToken)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(programToken);
            var token = cancellationTokenSource.Token;

            var file = new FileInfo(fileName);
            var stream = file.OpenRead();

            const int byteToMegabyteFactor = 1000000;
            var pageSize = 8 * byteToMegabyteFactor;

            var taskPoolScheduler =
                System.Reactive.Concurrency.Scheduler.Default; //new TaskPoolScheduler(new TaskFactory(token));
            var pageObservable = Observable.While(
                // while there is data to be read
                () => stream.CanRead && stream.Position < stream.Length,
                // iteratively invoke the observable factory, which will
                // "recreate" it such that it will start from the current
                // stream position - hence "0" for offset
                Observable.FromAsync(async () =>
                    {
                        var buffer = new byte[pageSize];
                        var readBytes = await stream.ReadAsync(buffer, 0, pageSize, token).ConfigureAwait(false);
                        return new
                        {
                            readBytes,
                            startIndex = stream.Position - readBytes,
                            buffer
                        };
                    })
                    .ObserveOn(taskPoolScheduler)
                    .Select(readResult =>
                    {
                        return new
                        {
                            array = readResult.buffer.Take(readResult.readBytes).ToArray(),
                            startPayloadIndex = readResult.startIndex
                        };
                    }));

            ///*await*/ var pageTest = pageObservable
            //    .ObserveOn(taskPoolScheduler)
            //    .Select((x, count) =>
            //{
            //    System.Console.WriteLine($"{count} {Newtonsoft.Json.JsonConvert.SerializeObject(new { array = x.array.Take(3), x.array.Length, startIndex = x.startPayloadIndex }, Formatting.Indented)}");
            //    return x;
            //}); //.ToTask(token).ConfigureAwait(false);

            const long typicalMaxTransmitUnit = 1400;
            const long broadcastIdByteCount = 16; //guid bytes
            const long fudgeAmount = 100;
            var packetSize = typicalMaxTransmitUnit - broadcastIdByteCount - fudgeAmount;
            var partialPacketCache = new ConcurrentDictionary<long, byte[]>();
            var payloadObservable = pageObservable
                .ObserveOn(taskPoolScheduler)
                .SelectMany(page =>
                {
                    var firstPacketIndex = page.startPayloadIndex / packetSize;
                    var hasFirstFragmented = (page.startPayloadIndex % packetSize) != 0;
                    var packetList = new List<PayloadWrapper>();

                    var firstPartialPacketIndex = hasFirstFragmented
                        ? firstPacketIndex
                        : (long?) null;
                    var secondPacketPayloadIndex = (firstPartialPacketIndex + 1) * packetSize;
                    var firstPartialLength = (secondPacketPayloadIndex - page.startPayloadIndex) ?? 0;

                    if (hasFirstFragmented)
                    {
                        var partialBuffer = new byte[firstPartialLength];
                        Array.Copy(page.array, partialBuffer, firstPartialLength);
                        var firstPartialPacket = partialBuffer;
                        var firstPayload = new PayloadWrapper()
                            {PayloadIndex = firstPartialPacketIndex.Value, bytes = firstPartialPacket};
                        CachePartialPacket(partialPacketCache, firstPayload, packetList);
                    }

                    var firstFullPacketIndex = hasFirstFragmented
                        ? firstPacketIndex + 1
                        : firstPacketIndex;

                    var lastPageByteIndex = page.array.Length - 1;
                    var lastBytePayloadIndex = page.startPayloadIndex + lastPageByteIndex;
                    var lastPacketIndex = lastBytePayloadIndex / packetSize;
                    var lastPartialLength = (page.array.Length - firstPartialLength) % packetSize;
                    var hasLastPartialPacket = lastPacketIndex > firstPacketIndex &&
                                               (lastPartialLength > 0);
                    var lastFullPacketIndex = hasLastPartialPacket
                        ? lastPacketIndex - 1
                        : lastPacketIndex;

                    //todo: consider parallel foreach
                    for (long packetIndex = firstFullPacketIndex; packetIndex <= lastFullPacketIndex; packetIndex++)
                    {
                        var packetBuffer = new byte[packetSize];
                        var startPageIndex = ((packetIndex - firstFullPacketIndex) * packetSize) + firstPartialLength;
                        Array.Copy(page.array, startPageIndex, packetBuffer, 0, packetSize);
                        var payload = new PayloadWrapper() {PayloadIndex = packetIndex, bytes = packetBuffer};
                        packetList.Add(payload);
                    }

                    if (hasLastPartialPacket)
                    {
                        var partialBuffer = new byte[lastPartialLength];
                        var lastPartialPageIndex = page.array.Length - lastPartialLength;
                        var lastPartialPacketIndex = lastPacketIndex;
                        Array.Copy(page.array, lastPartialPageIndex, partialBuffer, 0, lastPartialLength);
                        var lastPayload = new PayloadWrapper()
                            {PayloadIndex = lastPartialPacketIndex, bytes = partialBuffer};
                        CachePartialPacket(partialPacketCache, lastPayload, packetList);
                    }

                    return packetList.AsEnumerable();
                })
                .Concat(partialPacketCache
                    .Select(kvp => new PayloadWrapper() {PayloadIndex = kvp.Key, bytes = kvp.Value}).ToObservable());
            //try
            //{
            //    await payloadObservable
            //        .ObserveOn(taskPoolScheduler)
            //        .Select((array, index) =>
            //        {
            //            try
            //            {
            //                System.Console.WriteLine(
            //                    $"{index} {JsonConvert.SerializeObject(new {array = array.Take(10).ToArray(), array.Length}, Formatting.Indented)}");
            //            }
            //            catch (Exception e)
            //            {
            //                System.Console.WriteLine($"{index} .error writing line {e}");
            //            }

            //            return 0;
            //        })
            //        .ToTask(token).ConfigureAwait(false);
            //}
            //catch (Exception e)
            //{
            //    System.Console.WriteLine(e);
            //}

            var broadcastId = Guid.NewGuid();

            var serializedPayloadObservable = payloadObservable
                .Select(payloadWrapper =>
                {
                    byte[] serializedPayload;
                    var packetCount = (long) Math.Ceiling((double) file.Length / packetSize);
                    var rebroadcastTime = TimeSpan.FromSeconds(1);
                    using (var memoryStream = new MemoryStream())
                    {
                        Serializer.Serialize(memoryStream,
                            new ProtoMessage(broadcastId, payloadWrapper.PayloadIndex, payloadWrapper.bytes, null, null,
                                null));
                        serializedPayload = memoryStream.ToArray();
                    }

                    return serializedPayload;
                });


            byte[] serializedHeader;
            var packetCount = (long) Math.Ceiling((double) file.Length / packetSize);
            var rebroadcastTime = TimeSpan.FromSeconds(1);
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream,
                    new ProtoMessage(broadcastId, null, null, packetSize, fileName, packetCount));
                serializedHeader = memoryStream.ToArray();
            }

            var headerObservable = Observable.Return(serializedHeader)
                .Concat(
                    Observable.Interval(rebroadcastTime, Scheduler.Default)
                        .TakeUntil(serializedPayloadObservable.LastOrDefaultAsync())
                        .Select(_ => serializedHeader)
                )
                .Concat(Observable.Return(serializedHeader));

            var packetObservable = headerObservable
                .Merge(serializedPayloadObservable);

            UdpClient sender = new UdpClient(port);
            IPEndPoint destination = new IPEndPoint(IPAddress.Broadcast, port);

            await packetObservable
                .ObserveOn(taskPoolScheduler)
                .SelectMany((array, index) =>
                {
                    return Observable.FromAsync(async c =>
                    {
                        await sender.SendAsync(array, array.Length, destination).ConfigureAwait(false);
                        //System.Console.WriteLine(
                        //    $"{index} {JsonConvert.SerializeObject(new {array = array.Take(10).ToArray(), array.Length}, Formatting.Indented)}");
                        return Unit.Default;
                    });
                })
                .ToTask(token).ConfigureAwait(false);

        }

        private static void CachePartialPacket(ConcurrentDictionary<long, byte[]> partialPacketCache,
            PayloadWrapper payload,
            List<PayloadWrapper> packetList)
        {
            var shouldDelete = false;
            var r = partialPacketCache.AddOrUpdate(payload.PayloadIndex,
                payload.bytes,
                (_, cachedPartial) =>
                {
                    shouldDelete = true;
                    var packetBuffer = new byte[cachedPartial.Length + payload.bytes.Length];
                    Array.Copy(cachedPartial, packetBuffer, cachedPartial.Length);
                    Array.Copy(payload.bytes, 0, packetBuffer, cachedPartial.Length, payload.bytes.Length);
                    packetList.Add(payload);
                    return cachedPartial;
                });
            if (shouldDelete)
            {
                partialPacketCache.TryRemove(payload.PayloadIndex, out _);
            }
        }

        private async Task Listen(int port,
            CancellationToken programToken = default,
            Action<string> fileCompleteCallback = null)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(programToken);
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

                    System.Console.WriteLine(
                        $"id:{new Guid(message.BroadcastId)} {JsonConvert.SerializeObject(message)}");
                    return message;
                });

            var headerCache = new ConcurrentDictionary<Guid, ProtoMessage>();
            var payloadCache = new ConcurrentDictionary<Guid, List<ProtoMessage>>();

            var fileWriteRequestSubject = new Subject<FileWriteWrapper>();

            var headerObservable = protoMessageObservable
                .Where(message => !string.IsNullOrWhiteSpace(message.FileName) && message.PayloadCount.HasValue);

            var payloadObservable = protoMessageObservable
                .Where(message => message.PayloadIndex.HasValue && message.Payload != null);

            headerObservable
                .ObserveOn(observableScheduler)
                .Subscribe(header => headerCache.AddOrUpdate(header.GetBroadcastId(),
                    bcid =>
                    {
                        if (payloadCache.TryRemove(bcid, out var payloads))
                        {
                            foreach (var payload in payloads)
                            {
                                fileWriteRequestSubject.OnNext(new FileWriteWrapper() {Header = header, Payload = payload});
                            }
                        }
                        return header;
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
                .ObserveOn(observableScheduler)
                .SelectMany(writeRequest =>
                {
                    return Observable.FromAsync(async () =>
                    {
                        var file = new FileInfo($"{System.DateTime.Now:HHmmssffff}.{writeRequest.Header.FileName}");
                        var stream = file.OpenWrite();
                        var writeIndex = writeRequest.Payload.PayloadIndex.Value * writeRequest.Header.PayloadMaxSize.Value;
                        stream.Seek(writeIndex, SeekOrigin.Begin);
                        await stream.WriteAsync(writeRequest.Payload.Payload, token).ConfigureAwait(false);
                        return writeRequest.Header;
                    });
                });

            var fileWriteObservable = writeCompleteObservable
                .ObserveOn(observableScheduler)
                .GroupBy(w => w.GetBroadcastId());

            var fileTimeout = TimeSpan.FromSeconds(10);
            Func<IGroupedObservable<Guid, ProtoMessage>, IObservable<ProtoMessage>> createAmb = null;
            IObservable<ProtoMessage> CreateAmb(IGroupedObservable<Guid, ProtoMessage> g)
            {
                var nextAmb = g.FirstOrDefaultAsync().Select(x=>createAmb(g)).Switch();
                var timeoutObservable = (IObservable<ProtoMessage>)Observable.Return(g).FirstOrDefaultAsync().Delay(fileTimeout);
                return Observable.Amb( nextAmb, timeoutObservable);
            }
            createAmb = CreateAmb;

            var fileStoppedObservable = fileWriteObservable
                .SelectMany(g => CreateAmb(g))
                .Select(p =>
                {
                    fileCompleteCallback?.Invoke(p.FileName);
                    return p.FileName;
                });

            var fileStoppedSub = fileStoppedObservable.Subscribe();
            token.Register(() => fileStoppedSub.Dispose());

            UdpClient receiver = new UdpClient(port);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            while (!token.IsCancellationRequested)
            {
                var received = await receiver.ReceiveAsync().ConfigureAwait(false);
                System.Console.WriteLine($"Buff bytes {received.Buffer.Length}");
                packetBufferObservable.OnNext(received.Buffer);
            }
        }
    }

    internal record FileWriteWrapper {
        internal ProtoMessage Header { get; init; }
        internal ProtoMessage Payload { get; init; }
    }

    internal record PayloadWrapper
    {
        internal long PayloadIndex { get; init; }
        internal byte[] bytes { get; init; }
    }

    /// <summary>
    /// This message class is specific to the serialization library being used in this service, ProtocolBuffers. This represents the shape of the data that is actually sent as bytes over the network.
    /// </summary>
    [ProtoContract]
    internal class ProtoMessage
    {
        [ProtoMember(1)]
        public byte[] BroadcastId { get; set; }
        [ProtoMember(2)]
        public long? PayloadIndex { get; set; }
        [ProtoMember(3)]
        public byte[] Payload { get; set; }
        [ProtoMember(4)]
        public long? PayloadMaxSize { get; set; }
        [ProtoMember(5)]
        public string FileName { get; set; }
        [ProtoMember(6)]
        public long? PayloadCount { get; set; }
            
        /// <summary>
        /// Parameterless constructor required for protocol buffer deserialization
        /// </summary>
        internal ProtoMessage() {}

        internal ProtoMessage(Guid broadcastId, long? payloadIndex, byte[] payload, long? payloadMaxSize, string fileName, long? payloadCount)
        {
            BroadcastId = broadcastId.ToByteArray();
            PayloadIndex = payloadIndex;
            Payload = payload;
            PayloadMaxSize = payloadMaxSize;
            FileName = fileName;
            PayloadCount = payloadCount;
        }

        internal Guid GetBroadcastId()
        {
            return new Guid(BroadcastId);
        }
    }
}
