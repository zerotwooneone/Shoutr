namespace Library.Configuration
{
    /// <summary>
    /// Provides access to common configuration values
    /// </summary>
    public interface IConfigurationService
    {
        int BroadcastPort { get; }
        int? MaxBroadcastsPerSecond { get; }
        uint PageSize { get; }
    }
}