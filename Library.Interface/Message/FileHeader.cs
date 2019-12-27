using System;

namespace Library.Interface.Message
{
    public class FileHeader : MessageHeader, IFileHeader
    {
        public FileHeader(Guid broadcastId, bool? isLast, string fileName, long firstPayloadIndex) : base(broadcastId, isLast)
        {
            FileName = fileName;
            FirstPayloadIndex = firstPayloadIndex;
        }

        public string FileName { get; }
        public long FirstPayloadIndex { get; }

        public override int GetHashCode()
        {
            return BroadcastMessage.GetHashCode((string) FileName, (int) base.GetHashCode());
        }
    }
}