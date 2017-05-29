namespace Library.Configuration
{
    public interface IConfigurationService
    {
        int BroadcastPort { get; }
        int? MaxBroadcastsPerSecond { get; }
        int PageSize { get; }
    }
}