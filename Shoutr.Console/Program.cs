using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shoutr.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");
            var tokenSource = new CancellationTokenSource();
            void OnCancelKey(object sender, ConsoleCancelEventArgs e)
            {
                tokenSource.Cancel();
            }
            System.Console.CancelKeyPress += OnCancelKey;
            var program = new Program();
            program.Do(tokenSource.Token)
                   .Wait(tokenSource.Token);
        }

        private Program(){ }

        private async Task Do(CancellationToken programToken)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(programToken);
            var token = cancellationTokenSource.Token;
            
            var fileName = "test.7z";
            var file = new FileInfo(fileName);
            var stream = file.OpenRead();

            const int byteToMegabyteFactor = 1000000;
            var pageSize = 8 * byteToMegabyteFactor;

            var taskPoolScheduler = System.Reactive.Concurrency.Scheduler.Default; //new TaskPoolScheduler(new TaskFactory(token));
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
            //}); //.ToTask(token);

            var packetSize = 1400;
            var partialPacketCache = new ConcurrentDictionary<long, byte[]>();
            var packetObservable = pageObservable
                .ObserveOn(taskPoolScheduler)
                .SelectMany(page =>
                {
                    var firstPacketIndex = page.startPayloadIndex / packetSize;
                    var hasFirstFragmented = (page.startPayloadIndex % packetSize) != 0;
                    var packetList = new List<byte[]>();
                    
                    var firstPartialPacketIndex = hasFirstFragmented
                        ? firstPacketIndex
                        : (long?)null;
                    var secondPacketPayloadIndex = (firstPartialPacketIndex + 1) * packetSize;
                    var firstPartialLength = (secondPacketPayloadIndex - page.startPayloadIndex) ?? 0;

                    if (hasFirstFragmented)
                    {
                        var partialBuffer = new byte[firstPartialLength];
                        Array.Copy(page.array, partialBuffer, firstPartialLength);
                        var firstPartialPacket = partialBuffer;
                        CachePartialPacket(partialPacketCache, firstPartialPacketIndex.Value, firstPartialPacket, packetList);
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
                        packetList.Add(packetBuffer);
                    }

                    if (hasLastPartialPacket)
                    {
                        var partialBuffer = new byte[lastPartialLength];
                        var lastPartialPageIndex = page.array.Length - lastPartialLength;
                        var lastPartialPacketIndex = lastPacketIndex;
                        Array.Copy(page.array, lastPartialPageIndex, partialBuffer, 0, lastPartialLength);
                        CachePartialPacket(partialPacketCache, lastPartialPacketIndex, partialBuffer, packetList);
                    }

                    return packetList.AsEnumerable();
                })
                .Concat(partialPacketCache.Select(kvp => kvp.Value).ToObservable());
            try
            {
                await packetObservable
                    .ObserveOn(taskPoolScheduler)
                    .Select((array, index) =>
                    {
                        try
                        {
                            System.Console.WriteLine(
                                $"{index} {JsonConvert.SerializeObject(new {array = array.Take(10).ToArray(), array.Length}, Formatting.Indented)}");
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine($"{index} .error writing line {e}");
                        }

                        return 0;
                    })
                    .ToTask(token);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }

            //UdpClient sender = new UdpClient(3036);
            //IPEndPoint destination = new IPEndPoint(IPAddress.Broadcast, 3036);
            //sender.Connect(destination);

        }

        private static void CachePartialPacket(ConcurrentDictionary<long, byte[]> partialPacketCache, long packetIndex, byte[] packet,
            List<byte[]> packetSubject)
        {
            var shouldDelete = false;
            var r = partialPacketCache.AddOrUpdate(packetIndex,
                packet,
                (_, cachedPartial) =>
                {
                    shouldDelete = true;
                    var packetBuffer = new byte[cachedPartial.Length + packet.Length];
                    Array.Copy(cachedPartial, packetBuffer, cachedPartial.Length);
                    Array.Copy(packet, 0, packetBuffer, cachedPartial.Length, packet.Length);
                    packetSubject.Add(packetBuffer);
                    return cachedPartial;
                });
            if (shouldDelete)
            {
                partialPacketCache.TryRemove(packetIndex, out _);
            }
        }
    }
}
