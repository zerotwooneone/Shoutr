using System;
using System.Reactive.Linq;
using Library.Message;

namespace Library.Listen
{
    public interface IListener
    {
        IObservable<IReceivedMessage> MessagesObservable { get; }
    }

    public static class ObservableMessagesExtensions
    {
        public static IObservable<IBroadcastHeader> GetHeaderObservable(this IObservable<IReceivedMessage> observable)
        {
            return observable
                .Where(m => m.BroadcastHeader != null)
                .Select(m => m.BroadcastHeader);
        }

        public static IObservable<IChunkHeader> GetChunkObservable(this IObservable<IReceivedMessage> observable)
        {
            return observable
                .Where(m => m.ChunkHeader != null)
                .Select(m => m.ChunkHeader);
        }

        public static IObservable<IFileHeader> GetFileObservable(this IObservable<IReceivedMessage> observable)
        {
            return observable
                .Where(m => m.FileHeader != null)
                .Select(m => m.FileHeader);
        }

        public static IObservable<IPayloadMessage> GetPayloadObservable(this IObservable<IReceivedMessage> observable)
        {
            return observable
                .Where(m => m.PayloadMessage != null)
                .Select(m => m.PayloadMessage);
        }
    }


}