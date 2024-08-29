using L4d2ServerQuery.Model;
using SteamQuery;
using SteamQuery.Models;

namespace L4d2ServerQuery.Service;

public class ServerQueryService
{
    public List<ServerInformation> Servers { get; } = new();
    private FavoriteServerContext _db = new();

    public void UpdateServers()
    {
        List<FavoriteServer> servers = _db.FavoriteServers.ToList();
        foreach (FavoriteServer server in servers)
        {
            ServerInformation serverInformation = new(server);
            Servers.Add(serverInformation);
        }
    }

    public void AddServer(FavoriteServer server)
    {
        _db.FavoriteServers.Add(server);
    }
}

public class ServerInformation
{

    private FavoriteServer _favoriteServer{ get; }
    private SteamQueryInformation? Information { get; set; } // 返回的查询信息

    private Timer _timer;
    
    private void InitializeTimer()
    {
        // 设置定时器的间隔时间和回调函数
        // 例如，每5分钟执行一次 Do 方法
        _timer = new Timer(Do, this, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
    

    public ServerInformation(FavoriteServer favoriteServer)
    {
        _favoriteServer = favoriteServer;
        InitializeTimer();
    }

    private async void Do(object? state)
    {
        string host = _favoriteServer.Addr;

        var server = new GameServer(host);

        await server.PerformQueryAsync();

        Information = server.Information;

        server.Close();
    }
    
    public SteamQueryInformation? GetSteamQueryInformation() => Information;
}