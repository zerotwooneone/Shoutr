namespace Shoutr.WebServer.Hubs;

public class HubPeer
{
    public HubPeer(string id, string nickname)
    {
        Id = id;
        Nickname = nickname;
    }

    public string Id { get; init; }
    public string Nickname { get; init; }
    public string? PublicKey { get; init; }
}