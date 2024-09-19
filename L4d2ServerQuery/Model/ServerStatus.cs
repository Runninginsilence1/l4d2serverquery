namespace L4d2ServerQuery.Model;

public class ServerStatus
{
    public int Id { get; set; }
    public string? Address { get; set; }
    public string? ServerName { get; set; }
    public string? Map { get; set; }
    public int OnlinePlayers { get; set; }
    public int MaxPlayers { get; set; }
    
}