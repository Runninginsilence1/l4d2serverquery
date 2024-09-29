using SteamQuery.Models;

namespace L4d2ServerQuery.Model;

// 这个应该差不多就是dto的感觉
public class ServerStatusDto
{
    public int Id { get; set; }
    public string? Address { get; set; }
    public string? ServerName { get; set; }
    public string? Map { get; set; }
    public int OnlinePlayers { get; set; }
    public int MaxPlayers { get; set; }

    public ServerStatusDto() {}

    public ServerStatusDto(FavoriteServer? favoriteServer, SteamQueryInformation? information)
    {
        if (favoriteServer is null) return;
        Id = favoriteServer.Id;
        Address = favoriteServer?.Addr;
        ServerName = information?.ServerName;
        Map = information?.Map;
        OnlinePlayers = information?.OnlinePlayers?? 0;
        MaxPlayers = information?.MaxPlayers?? 0;
    }
}

public class TagDto
{
    
}