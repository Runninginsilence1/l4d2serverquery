using System.Diagnostics;
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
                    SendTimeout = 3000,
                    ReceiveTimeout = 3000,
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

        const int expectedPlayers = 8;

        var result = status.OrderBy(s => Math.Abs(s.OnlinePlayers - expectedPlayers)).ToList();
        Stopwatch.Stop();
        Console.WriteLine($"查询了 {count} 个服务器, 用时: {Stopwatch.ElapsedMilliseconds} ms");
        return result;
    }
}