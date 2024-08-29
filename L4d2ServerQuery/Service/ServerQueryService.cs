using L4d2ServerQuery.Model;
using SteamQuery;
using SteamQuery.Models;

namespace L4d2ServerQuery.Service;

public class ServerQueryService
{
    public List<ServerInformation>? Servers { get; set; }
    

    public void Do()
    {
        
    }
    
    
}

public class ServerInformation
{
    private int _counter = 0;
    
    public FavoriteServer? FavoriteServer { get; set; } // 基础信息
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
        FavoriteServer = favoriteServer;
        
    }

    public async void Do(object? state)
    {
        Console.WriteLine($"{_counter}th Do");
        _counter++;
        string host = FavoriteServer.Addr;

        var server = new GameServer(host);

        await server.PerformQueryAsync();

        Information = server.Information;

        server.Close();
    }
    
    public SteamQueryInformation? GetSteamQueryInformation() => Information;
}