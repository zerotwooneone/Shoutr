namespace Shoutr.Io
{
    public record Page
    {

        public long PageIndex { get; init; }
        public byte[] Bytes { get; init; }
    }
}