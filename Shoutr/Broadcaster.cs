using ProtoBuf;
using Shoutr.Contracts;
using Shoutr.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;

namespace Shoutr
{
    public class Broadcaster : IBroadcaster
    {
        public async Task BroadcastFile(string fileName, 
            IByteSender byteSender, 
            float headerRebroadcastSeconds = 1,
            CancellationToken cancellationToken = default)
        {
            
            if(headerRebroadcastSeconds < 0)
            {
                throw new ArgumentException(nameof(headerRebroadcastSeconds));
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
            //    DdsLog($"{count} {Newtonsoft.Json.JsonConvert.SerializeObject(new { array = x.array.Take(3), x.array.Length, startIndex = x.startPayloadIndex }, Formatting.Indented)}");
            //    return x;
            //}); //.ToTask(token).ConfigureAwait(false);

            const long broadcastIdByteCount = 16; //guid bytes
            const long fudgeAmount = 100;
            var packetSize = byteSender.MaximumTransmittableBytes - broadcastIdByteCount - fudgeAmount;
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
                    .Select(kvp => new PayloadWrapper {PayloadIndex = kvp.Key, bytes = kvp.Value}).ToObservable());
            
            var broadcastId = Guid.NewGuid();

            var serializedPayloadObservable = payloadObservable
                .Select(payloadWrapper =>
                {
                    byte[] serializedPayload;
                    var protoMessage = new ProtoMessage(broadcastId, payloadWrapper.PayloadIndex, payloadWrapper.bytes, null, null,
                        null);
                    using (var memoryStream = new MemoryStream())
                    {
                        Serializer.Serialize(memoryStream,
                            protoMessage);
                        serializedPayload = memoryStream.ToArray();
                    }
                    // DdsLog($"serialized packet {Newtonsoft.Json.JsonConvert.SerializeObject(new {array = serializedPayload.Take(10).ToArray(), serializedPayload.Length, payloadWrapper.PayloadIndex}, Newtonsoft.Json.Formatting.Indented)}",
                    //     true);
                    return serializedPayload;
                })
                .Finally(() =>
                {
                    DdsLog($"finally serializedPayloadObservable");
                })
                .Publish();
            
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
                        .Select(_ =>
                        {
                            DdsLog($"header {Newtonsoft.Json.JsonConvert.SerializeObject(new {array = serializedHeader.Take(10).ToArray(), serializedHeader.Length}, Newtonsoft.Json.Formatting.Indented)}",
                                true);
                            return serializedHeader;
                        })
                )
                .Concat(Observable.Return(serializedHeader))
                .Finally(() =>
                {
                    DdsLog($"headerObservable finally");
                });

            var packetObservable = headerObservable
                .Merge(serializedPayloadObservable)
                .Finally(() =>
                {
                    DdsLog($"packetObservable finally");
                });

            serializedPayloadObservable.Connect();

            var sendObservable = packetObservable
                .ObserveOn(taskPoolScheduler)
                .SelectMany((array, index) =>
                {
                    return Observable.FromAsync(async c =>
                    {
                        await byteSender.Send(array).ConfigureAwait(false);
                        // DdsLog($"after byteSender.Send  {index} {Newtonsoft.Json.JsonConvert.SerializeObject(new {array = array.Take(10).ToArray(), array.Length}, Newtonsoft.Json.Formatting.Indented)}",
                        //     true);
                        return Unit.Default;
                    });
                })
                .Finally(() =>
                {
                    DdsLog($"sendObservable finally");
                });
            await sendObservable
                .ToTask(token).ConfigureAwait(false);
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
    }
}

