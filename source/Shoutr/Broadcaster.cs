using Shoutr.Contracts;
using Shoutr.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Shoutr.Contracts.ByteTransport;
using Shoutr.Contracts.Io;
using Shoutr.Io;
using Shoutr.Reactive;

namespace Shoutr
{
    public class Broadcaster : IBroadcaster
    {
        private readonly ISchedulerLocator _schedulerLocator;

        public Broadcaster(ISchedulerLocator schedulerLocator)
        {
            _schedulerLocator = schedulerLocator;
        }
        public async Task BroadcastFile(string fileName,
            IByteSender byteSender,
            IStreamFactory streamFactory,
            float headerRebroadcastSeconds = 1,
            CancellationToken cancellationToken = default)
        {

            if (headerRebroadcastSeconds < 0)
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
                .ObserveOn(_schedulerLocator.GetScheduler("broadcast"))
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
                    var hash = md5.ComputeHash(payloadWrapper.bytes);
                    var protoMessage = new ProtoMessage
                    {
                        BroadcastId = ByteString.CopyFrom(broadcastId.ToByteArray()),
                        PayloadIndex = payloadWrapper.PayloadIndex,
                        Payload = ByteString.CopyFrom(payloadWrapper.bytes),
                        Hash = ByteString.CopyFrom(hash)
                    }; 

                    return protoMessage.ToByteArray();
                })
                .Finally(() => { md5.Dispose(); })
                .Publish();

            byte[] serializedHeader;
            var packetCount = (long) Math.Ceiling((double) reader.Length / packetSize);
            var rebroadcastTime = TimeSpan.FromSeconds(headerRebroadcastSeconds);
            var protoMessage = new ProtoMessage
            {
                BroadcastId = ByteString.CopyFrom(broadcastId.ToByteArray()),
                PayloadMaxSize = packetSize,
                FileName = fileName,
                PayloadCount = packetCount
            };

            var byteArray = protoMessage.ToByteArray();
            var headerObservable =
                GetHeaderObservable(byteArray, rebroadcastTime, serializedPayloadObservable);

            var packetObservable = headerObservable
                .Merge(serializedPayloadObservable);

            serializedPayloadObservable.Connect();

            var sendObservable = packetObservable
                .ObserveOn(_schedulerLocator.GetScheduler("broadcast packet"))
                .SelectMany((array, index) =>
                {
                    return Observable.FromAsync(async c =>
                    {
                        await byteSender.Send(array).ConfigureAwait(false);
                        return Unit.Default;
                    });
                });
            await sendObservable
                .ToTask(token).ConfigureAwait(false);
        }

        public IObservable<byte[]> GetHeaderObservable(byte[] serializedHeader,
            TimeSpan rebroadcastTime,
            IObservable<byte[]> payloadObservable)
        {
            return Observable
                .Return(serializedHeader)
                .Concat(
                    Observable.Interval(rebroadcastTime, _schedulerLocator.GetScheduler("broadcast header"))
                        .TakeUntil(payloadObservable.LastOrDefaultAsync())
                        .Select(_ => { return serializedHeader; })
                )
                .Concat(Observable.Return(serializedHeader));
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

