namespace Shoutr
{
    internal record PayloadWrapper
    {
        internal long PayloadIndex { get; init; }
        internal byte[] bytes { get; init; }
    }
}
