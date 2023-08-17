using Shoutr.Serialization;

namespace Shoutr
{
    internal record FileWriteWrapper
    {
        internal Header Header { get; init; }
        internal ProtoMessage Payload { get; init; }
    }
}