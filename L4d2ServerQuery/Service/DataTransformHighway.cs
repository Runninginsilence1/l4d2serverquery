using System.Text.Json.Serialization;
using L4d2ServerQuery.Data;
using L4d2ServerQuery.Model;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace L4d2ServerQuery.Service;

public static class DataTransformHighway
{
    private static string? FilePath { get; set; }

    public static void Import(string arg)
    {
        FilePath = arg;
        // 反序列化获取列表
        

        // 使用 newtonsoft 反序列化
        var favoriteServerJson = JsonConvert.DeserializeObject<List<FavoriteServerJson>>(File.ReadAllText(FilePath));


        {
            // 使用 System.Text.Json 反序列化
            // var fileStream = new FileStream(FilePath, FileMode.Open);
            // var favoriteServerJson = JsonSerializer.Deserialize<List<FavoriteServerJson>>(fileStream);

            // fileStream.Close();
        }
        


        // 获取数据库实例并将数据导入
        var serverContext = new ServerContext();

        var favoriteServers = favoriteServerJson.Select(oldServer => new FavoriteServer
        {
            CreateAt = oldServer.CreateAt,
            Host = oldServer.Host,
            Desc = oldServer.Desc,
            Port = oldServer.Port,
        }).ToList();

        // 使用范围添加
        // 确定是 AddRange吗?
        serverContext.AddRange(favoriteServers);

        // 一个个添加
        // foreach (FavoriteServer server in favoriteServers)
        // {
        //     serverContext.FavoriteServers.Add(server);
        // }

        serverContext.SaveChanges();
    }
}