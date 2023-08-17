using System;

namespace Shoutr.Serialization
{
    /// <summary>
    /// This message class is specific to the serialization library being used in this service, ProtocolBuffers. This represents the shape of the data that is actually sent as bytes over the network.
    /// </summary>
    public partial class ProtoMessage
    {
        internal Guid? GetBroadcastId()
        {
            if (!HasBroadcastId)
            {
                return null;
            }
            return new Guid(BroadcastId.ToByteArray());
        }

        internal string GetHashString()
        {
            if (!HasHash)
            {
                return null;
            }
            return Convert.ToBase64String(Hash.ToByteArray());
        }
    }
}
