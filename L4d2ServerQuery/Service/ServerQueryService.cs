using L4d2ServerQuery.Model;
using SteamQuery;
using SteamQuery.Models;

namespace L4d2ServerQuery.Service;

// 思路是: 维护一个自己有定时器来执行查询的类列表.
// 这样我只要获取List, 就能获取到所有的实时的服务器信息.

// 这个类里面的数据库查询是可以正常查找到数据的;
public class ServerQueryService
{
    private List<ServerInformation> Servers { get; set; } = new();
    private FavoriteServerContext _db = new();

    public ServerQueryService()
    {
        UpdateServers();
    }

    // 从数据库中获取最新的服务器列表, 然后重新更新 Servers 列表;
    public void UpdateServers()
    {
        Servers.Clear();
        List<FavoriteServer> servers = _db.FavoriteServers.ToList();
        Console.WriteLine($"UpdateServers 中, 收到{servers.Count}个服务器");
        if (servers.Count > 0)
        {
            Servers = servers.Select(server => new ServerInformation(server)).ToList();
        }
    }

    public List<ServerInformation> GetServersRealTime() => Servers;

}

public class ServerInformation
{
    
    // 使用 task 的cancelltoken 取消定时器的执行;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly FavoriteServer _favoriteServer;
    private SteamQueryInformation? Information { get; set; } // 返回的查询信息
    
    
    // 定义查询的异步任务
    // 异步查询, 更新字段, 然后关闭连接
    public async Task GetServerDataAsync()
    {
        var gameServer = new GameServer(_favoriteServer.Addr);
        
        Console.WriteLine($"准备尝试获取服务器信息, 底层ID: {_favoriteServer.Id}");

        // {
        //     await gameServer.PerformQueryAsync(); // 按道理来说这里应该有一个 cancellationToken 取消的功能;
        //     Information = gameServer.Information;            
        // }
        
        Console.WriteLine($"获取服务器信息成功, 底层ID: {_favoriteServer.Id}");
        
        gameServer.Close();
    }
    
    // 启动后台任务
    public void StartRefreshing()
    {
        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                Console.WriteLine("循环在运行");
                // test: 是否是你阻塞了
                await GetServerDataAsync();
                await Task.Delay(TimeSpan.FromSeconds(1), _cts.Token);
            }
        });
    }
    
    // 停止后台任务
    public void StopRefreshing()
    {
        _cts.Cancel();
    }
    
    
    private readonly FavoriteServerContext _dbCtx = new();

    public ServerInformation(FavoriteServer favoriteServer)
    {
        _favoriteServer = favoriteServer;
        StartRefreshing();
    }

    public List<FavoriteServer> RawServers()
    {
        return _dbCtx.FavoriteServers.ToList();
    }
    
    
    public SteamQueryInformation? GetSteamQueryInformation() => Information;

}