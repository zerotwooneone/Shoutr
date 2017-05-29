namespace Library.Configuration
{
    public interface IConfigurationService
    {
        int BroadcastPort { get; }
        int? MaxBroadcastsPerSecond { get; }
        uint PageSize { get; }
    }
}