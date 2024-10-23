using System.Diagnostics;
using L4d2ServerQuery.DTO;
using L4d2ServerQuery.Model;
using Serilog;
using SteamQuery;
using SteamQuery.Exceptions;
using SteamQuery.Models;

namespace L4d2ServerQuery.Service;

// 思路是: 维护一个自己有定时器来执行查询的类列表.
// 这样我只要获取List, 就能获取到所有的实时的服务器信息.

// 这个类里面的数据库查询是可以正常查找到数据的;

public static class QueryService
{
    private static readonly Stopwatch Stopwatch = new Stopwatch();

    public static async Task<List<ServerStatusDto>> Query(List<FavoriteServer> servers)
    {
        var status = new List<ServerStatusDto>();
        var tasks = new List<Task>();
        var count = 0;

        Stopwatch.Restart();

        foreach (var server in servers)
        {
            var host = server.Addr;

            GameServer gameServer;

            try
            {
                gameServer = new GameServer(host)
                {
                    SendTimeout = 1000,
                    ReceiveTimeout = 1000,
                };
            }
            catch (AddressNotFoundException e)
            {
                // Console.WriteLine($"{host} 是一个无效的地址");
                Log.Warning($"{host} 是一个无效的地址");
                continue;
            }


            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var info = await gameServer.GetInformationAsync();
                    count++;
                    // var s = new ServerStatusDto()
                    // {
                    //     Id = server.Id,
                    //     Address = host,
                    //     ServerName = info.ServerName,
                    //     Map = info.Map,
                    //     OnlinePlayers = info.OnlinePlayers,
                    //     MaxPlayers = info.MaxPlayers,
                    // };


                    var s = new ServerStatusDto(server, info);

                    status.Add(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{host} 无法连接");
                }
            }));
        }

        await Task.WhenAll(tasks);

        int expectedPlayers = 8;
        expectedPlayers = RankStore.Rank;

        var result = status.
            OrderBy(s => Math.Abs(s.OnlinePlayers - expectedPlayers)).
            ThenByDescending(s => s.LastQueryTime, new DateTimeComparer()).
            ToList();
        
        // 网上看的, 使用 sort 另外排序, 不使用 linq
        // result.Sort((s1, s2) => DateTime.Compare(s1.LastQueryTime?? DateTime.MinValue, s2.LastQueryTime?? DateTime.MinValue)); // 使用问号操作符做常量值
        
        Stopwatch.Stop();
        Console.WriteLine($"查询了 {count} 个服务器, 用时: {Stopwatch.ElapsedMilliseconds} ms");
        return result;
    }
    
    public static async Task<List<ServerStatusDto>> Query(List<FavoriteServer> servers, int rank)
    {
        var status = new List<ServerStatusDto>();
        var tasks = new List<Task>();
        var count = 0;

        Stopwatch.Restart();

        foreach (var server in servers)
        {
            var host = server.Addr;

            GameServer gameServer;

            try
            {
                gameServer = new GameServer(host)
                {
                    SendTimeout = 1000,
                    ReceiveTimeout = 1000,
                };
            }
            catch (AddressNotFoundException e)
            {
                // Console.WriteLine($"{host} 是一个无效的地址");
                Log.Warning($"{host} 是一个无效的地址");
                continue;
            }


            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var info = await gameServer.GetInformationAsync();
                    count++;
                    // var s = new ServerStatusDto()
                    // {
                    //     Id = server.Id,
                    //     Address = host,
                    //     ServerName = info.ServerName,
                    //     Map = info.Map,
                    //     OnlinePlayers = info.OnlinePlayers,
                    //     MaxPlayers = info.MaxPlayers,
                    // };


                    var s = new ServerStatusDto(server, info);

                    status.Add(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{host} 无法连接");
                }
            }));
        }

        await Task.WhenAll(tasks);

        int expectedPlayers = 8;
        expectedPlayers = rank;

        var result = status.
            OrderBy(s => Math.Abs(s.OnlinePlayers - expectedPlayers)).
            ThenByDescending(s => s.LastQueryTime, new DateTimeComparer()).
            ToList();
        
        // 网上看的, 使用 sort 另外排序, 不使用 linq
        // result.Sort((s1, s2) => DateTime.Compare(s1.LastQueryTime?? DateTime.MinValue, s2.LastQueryTime?? DateTime.MinValue)); // 使用问号操作符做常量值
        
        Stopwatch.Stop();
        Console.WriteLine($"查询了 {count} 个服务器, 用时: {Stopwatch.ElapsedMilliseconds} ms");
        return result;
    }

    public static async Task<List<PlayerListDto>> QueryPlayers(string addr)
    {
        GameServer gameServer;
        try
        {
            gameServer = new GameServer(addr)
            {
                SendTimeout = 3000,
                ReceiveTimeout = 3000,
            };
        }
        catch (AddressNotFoundException e)
        {
            // Console.WriteLine($"{host} 是一个无效的地址");
            Log.Warning($"{addr} 是一个无效的地址");
            throw;
        }

        var steamQueryPlayers = await gameServer.GetPlayersAsync();
        // 没有为结果数据生成id, 因为没有必要
        var playerListDtos = steamQueryPlayers.Select(p => new PlayerListDto(p));

        return playerListDtos.ToList();
    }
}

// 定义自定义的比较器, 用来排序时间
// 现在是时间越大的排越前面
class DateTimeComparer : IComparer<DateTime?>
{
    
    public int Compare(DateTime? x, DateTime? y)
    {
        // 如果 x 为空，则视为小于 y（除非 y 也为空）
        if (x == null)
        {
            return y == null ? 0 : 1;
        }
        
        if (y == null)
        {
            return 1;
        }

        // 如果都不为空，则使用 DateTime 默认的比较规则
        return -1 * x.Value.CompareTo(y.Value);
    }
}