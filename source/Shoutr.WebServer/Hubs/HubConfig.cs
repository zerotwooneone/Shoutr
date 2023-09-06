namespace Shoutr.WebServer.Hubs;

public class HubConfig
{
    public HubConfig(string userId, string userPublicKey)
    {
        UserId = userId;
        UserPublicKey = userPublicKey;
    }

    public string UserId { get; init; }
    public string UserPublicKey { get; init; }
}