namespace Shoutr.WebServer.Hubs;

public class HubPeer
{
    public string Id { get; init; }
    public string Nickname { get; init; }
    public string? PublicKey { get; init; }
}