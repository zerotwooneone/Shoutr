using ProtoBuf;
using Shoutr.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Shoutr
{
    public class Broadcaster : IBroadcaster
    {
        public async Task BroadcastFile(string fileName, 
            int port = 3036, 
            int mtu = 1400, 
            float headerRebroadcastSeconds = 1,
            string subnet = "192.168.1.255",
            CancellationToken cancellationToken = default)
        {
            const int minMtu = 1;
            if (mtu < minMtu)
            {
                throw new ArgumentOutOfRangeException(nameof(mtu));
            }
            const int minPort = 1;
            const int maxPort = 65535;
            if(port < minPort || port > maxPort)
            {
                throw new ArgumentException(nameof(port));
            }
            if(headerRebroadcastSeconds < 0)
            {
                throw new ArgumentException(nameof(headerRebroadcastSeconds));
            }
            if(!IPAddress.TryParse("192.168.1.255", out var subnetIpAddress))
            {
                throw new ArgumentException(nameof(subnetIpAddress));
            }

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
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

            const long broadcastIdByteCount = 16; //guid bytes
            const long fudgeAmount = 100;
            var packetSize = mtu - broadcastIdByteCount - fudgeAmount;
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
            
            var broadcastId = Guid.NewGuid();

            var serializedPayloadObservable = payloadObservable
                .Select(payloadWrapper =>
                {
                    byte[] serializedPayload;
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
            var rebroadcastTime = TimeSpan.FromSeconds(headerRebroadcastSeconds);
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream,
                    new ProtoMessage(broadcastId, null, null, packetSize, fileName, packetCount));
                serializedHeader = memoryStream.ToArray();
            }

            var headerObservable = Observable.Return(serializedHeader)
                .Concat(
                    Observable.Interval(rebroadcastTime, taskPoolScheduler)
                        .TakeUntil(serializedPayloadObservable.LastOrDefaultAsync())
                        .Select(_ => serializedHeader)
                )
                .Concat(Observable.Return(serializedHeader));

            var packetObservable = headerObservable
                .Merge(serializedPayloadObservable);

            UdpClient sender = new UdpClient(port);
            sender.EnableBroadcast = true; //may not be needed
            IPEndPoint destination = new IPEndPoint(subnetIpAddress, port);

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

    internal record PayloadWrapper
    {
        internal long PayloadIndex { get; init; }
        internal byte[] bytes { get; init; }
    }
    }

    
}
