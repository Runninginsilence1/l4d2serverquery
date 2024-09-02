using L4d2ServerQuery.Model;
using SteamQuery;
using SteamQuery.Models;

namespace L4d2ServerQuery.Service;

// 思路是: 维护一个自己有定时器来执行查询的类列表.
// 这样我只要获取List, 就能获取到所有的实时的服务器信息.

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
        _db.Add(server);
        _db.SaveChanges();
    }
}

public class ServerInformation: IDisposable, IAsyncDisposable
{

    private FavoriteServer _favoriteServer{ get; }
    private SteamQueryInformation? Information { get; set; } // 返回的查询信息

    private Timer _timer;
    
    private void InitializeTimer()
    {
        // 设置定时器的间隔时间和回调函数
        _timer = new Timer(Do, this, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
    private readonly FavoriteServerContext _dbCtx = new();

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

    public List<FavoriteServer> RawServers()
    {
        return _dbCtx.FavoriteServers.ToList();
    }
    
    public SteamQueryInformation? GetSteamQueryInformation() => Information;

    public void Dispose()
    {
        _timer.Dispose();
        _dbCtx.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _timer.DisposeAsync();
        await _dbCtx.DisposeAsync();
    }
}