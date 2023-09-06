namespace Shoutr.WebServer.Hubs;

public class HubBroadcast
{
    public HubBroadcast(string id)
    {
        Id = id;
    }

    public string Id { get; init; }
    public bool Completed { get; init; }
    public int? PercentComplete { get; init; }
}