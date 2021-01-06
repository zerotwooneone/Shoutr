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
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;
using Shoutr.Contracts.Io;
using Shoutr.Io;

namespace Shoutr
{
    public class Broadcaster : IBroadcaster
    {
        public async Task BroadcastFile(string fileName, 
            IByteSender byteSender, 
            IStreamFactory streamFactory,
            float headerRebroadcastSeconds = 1,
            CancellationToken cancellationToken = default)
        {
            var taskPoolScheduler =
                Scheduler.Default; //new TaskPoolScheduler(new TaskFactory(token)); 
            await BroadcastFile(fileName,
                byteSender,
                streamFactory,
                taskPoolScheduler,
                headerRebroadcastSeconds,
                cancellationToken);
        }
        
        public async Task BroadcastFile(string fileName, 
            IByteSender byteSender, 
            IStreamFactory streamFactory,
            IScheduler scheduler,
            float headerRebroadcastSeconds = 1,
            CancellationToken cancellationToken = default)
        {
            
            if(headerRebroadcastSeconds < 0)
            {
                throw new ArgumentException(nameof(headerRebroadcastSeconds));
            }
            
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = cancellationTokenSource.Token;

            var reader = streamFactory.CreateReader(fileName);

            const int byteToMegabyteFactor = 1000000;
            var pageSize = 8 * byteToMegabyteFactor;

            var pageObservable = reader.PageObservable(pageSize, token);
            
            const long broadcastIdByteCount = 16; //guid bytes
            const long fudgeAmount = 100; //the hash alg chosen has to be equal to or smaller than this
            var packetSize = byteSender.MaximumTransmittableBytes - broadcastIdByteCount - fudgeAmount;
            var partialPacketCache = new ConcurrentDictionary<long, byte[]>();
            var payloadObservable = pageObservable
                .ObserveOn(scheduler)
                .SelectMany(page =>
                {
                    var firstPacketIndex = page.PageIndex / packetSize;
                    var hasFirstFragmented = (page.PageIndex % packetSize) != 0;
                    var packetList = new List<PayloadWrapper>();

                    var firstPartialPacketIndex = hasFirstFragmented
                        ? firstPacketIndex
                        : (long?) null;
                    var secondPacketPayloadIndex = (firstPartialPacketIndex + 1) * packetSize;
                    var firstPartialLength = (secondPacketPayloadIndex - page.PageIndex) ?? 0;

                    if (hasFirstFragmented)
                    {
                        var partialBuffer = new byte[firstPartialLength];
                        Array.Copy(page.Bytes, partialBuffer, firstPartialLength);
                        var firstPartialPacket = partialBuffer;
                        var firstPayload = new PayloadWrapper()
                            {PayloadIndex = firstPartialPacketIndex.Value, bytes = firstPartialPacket};
                        CachePartialPacket(partialPacketCache, firstPayload, packetList);
                    }

                    var firstFullPacketIndex = hasFirstFragmented
                        ? firstPacketIndex + 1
                        : firstPacketIndex;

                    var lastPageByteIndex = page.Bytes.Length - 1;
                    var lastBytePayloadIndex = page.PageIndex + lastPageByteIndex;
                    var lastPacketIndex = lastBytePayloadIndex / packetSize;
                    var lastPartialLength = (page.Bytes.Length - firstPartialLength) % packetSize;
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
                        Array.Copy(page.Bytes, startPageIndex, packetBuffer, 0, packetSize);
                        var payload = new PayloadWrapper() {PayloadIndex = packetIndex, bytes = packetBuffer};
                        packetList.Add(payload);
                    }

                    if (hasLastPartialPacket)
                    {
                        var partialBuffer = new byte[lastPartialLength];
                        var lastPartialPageIndex = page.Bytes.Length - lastPartialLength;
                        var lastPartialPacketIndex = lastPacketIndex;
                        Array.Copy(page.Bytes, lastPartialPageIndex, partialBuffer, 0, lastPartialLength);
                        var lastPayload = new PayloadWrapper()
                            {PayloadIndex = lastPartialPacketIndex, bytes = partialBuffer};
                        CachePartialPacket(partialPacketCache, lastPayload, packetList);
                    }

                    return packetList.AsEnumerable();
                })
                .Concat(partialPacketCache
                    .Select(kvp => new PayloadWrapper {PayloadIndex = kvp.Key, bytes = kvp.Value}).ToObservable());
            
            var broadcastId = Guid.NewGuid();

            var md5 = MD5.Create();
            var serializedPayloadObservable = payloadObservable
                .Select(payloadWrapper =>
                {
                    byte[] serializedPayload;
                    var hash = md5.ComputeHash(payloadWrapper.bytes);
                    var protoMessage = new ProtoMessage(broadcastId, payloadWrapper.PayloadIndex, payloadWrapper.bytes, null, null,
                        null, hash);
                    using (var memoryStream = new MemoryStream())
                    {
                        Serializer.Serialize(memoryStream,
                            protoMessage);
                        serializedPayload = memoryStream.ToArray();
                    }

                    // var obj = new
                    // {
                    //     payloadWrapper.PayloadIndex,
                    //     hash = protoMessage.GetHashString(),
                    //     array = serializedPayload.Take(10).ToArray(), 
                    //     serializedPayload.Length,
                    // };
                    //DdsLog($"serialized packet {Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None)}");
                    return serializedPayload;
                })
                .Finally(() =>
                {
                    //DdsLog($"finally serializedPayloadObservable");
                    md5.Dispose();
                })
                .Publish();
            
            byte[] serializedHeader;
            var packetCount = (long) Math.Ceiling((double) reader.Length / packetSize);
            var rebroadcastTime = TimeSpan.FromSeconds(headerRebroadcastSeconds);
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream,
                    new ProtoMessage(broadcastId, null, null, packetSize, fileName, packetCount, null));
                serializedHeader = memoryStream.ToArray();
            }

            var headerObservable = GetHeaderObservable(serializedHeader, rebroadcastTime, serializedPayloadObservable, scheduler);

            var packetObservable = headerObservable
                .Merge(serializedPayloadObservable)
                // .Finally(() =>
                // {
                //     DdsLog($"packetObservable finally");
                // })
                ;

            serializedPayloadObservable.Connect();

            var sendObservable = packetObservable
                .ObserveOn(scheduler)
                .SelectMany((array, index) =>
                {
                    return Observable.FromAsync(async c =>
                    {
                        await byteSender.Send(array).ConfigureAwait(false);
                        // DdsLog($"after byteSender.Send  {index} {Newtonsoft.Json.JsonConvert.SerializeObject(new {array = array.Take(10).ToArray(), array.Length}, Newtonsoft.Json.Formatting.Indented)}",
                        //      true);
                        return Unit.Default;
                    });
                })
                // .Finally(() =>
                // {
                //     DdsLog($"sendObservable finally");
                // })
                ;
            await sendObservable
                .ToTask(token).ConfigureAwait(false);
        }

        public IObservable<byte[]> GetHeaderObservable(byte[] serializedHeader, 
            TimeSpan rebroadcastTime, 
            IObservable<byte[]> payloadObservable, 
            IScheduler scheduler)
        {
            return Observable
                .Return(serializedHeader)
                // .Select(h =>
                // {
                //     DdsLog($"first header {Newtonsoft.Json.JsonConvert.SerializeObject(new {array = serializedHeader.Take(10).ToArray(), serializedHeader.Length})}");
                //     return h;
                // })
                .Concat(
                    Observable.Interval(rebroadcastTime, scheduler)
                        .TakeUntil(payloadObservable.LastOrDefaultAsync())
                        .Select(_ =>
                        {
                            // DdsLog($"header {Newtonsoft.Json.JsonConvert.SerializeObject(new {array = serializedHeader.Take(10).ToArray(), serializedHeader.Length})}",
                            //     true);
                            return serializedHeader;
                        })
                )
                .Concat(Observable.Return(serializedHeader))
                    // .Select(h =>
                    // {
                    //     DdsLog($"last header {Newtonsoft.Json.JsonConvert.SerializeObject(new {array = serializedHeader.Take(10).ToArray(), serializedHeader.Length})}");
                    //     return h;
                    // })
                // .Finally(() =>
                // {
                //     DdsLog($"headerObservable finally");
                // })
                ;
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

