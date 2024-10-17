using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using L4d2ServerQuery.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace L4d2ServerQuery.Model;

// Code first: 使用以下的命令
/*
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design

主要运行下面这个:
dotnet ef migrations add InitialCreate
dotnet ef database update
 * 
 */
// public class FavoriteServerContext : DbContext
// {
//     
//     public DbSet<FavoriteServer> FavoriteServers { get; set; }
//
//     private string DbPath { get; }
//
//     public FavoriteServerContext()
//     {
//         var folder = Environment.SpecialFolder.LocalApplicationData;
//         var path = Environment.GetFolderPath(folder);
//         DbPath = Path.Join(path, "db.db");
//     }
//
//     // The following configures EF to create a Sqlite database file in the
//     // special "local" folder for your platform.
//     // 重写下面的方法, 使得EF可以创建SQLite数据库文件
//     protected override void OnConfiguring(DbContextOptionsBuilder options)
//         => options.UseSqlite($"Data Source={DbPath}");
// }

// 数据库实体




public class FavoriteServer
{
    
    [Key]
    public int Id { get; set; } // 主键按照约定, 名字一般要叫 classNameId 或者 直接 Id
    // public int ServerId { get; set; } // 或者通过 Key 属性来指定主键
    public DateTime CreateAt { get; set; } = DateTime.Now;
    
    [Required]
    public string? Host { get; set; }
    [Required]
    public int Port { get; set; }
    
    [MaxLength(255)]
    [Required]
    public string? Desc { get; set; }

    public string Addr => $"{Host}:{Port}";
    
    // 最后连接时间，用于作为排序的依据
    public DateTime? LastQueryAt { get; set; }
    
    // 查询字段，在查询后，调用这个方法， 将包含指定关键字的信息的修改指定描述
    public string? ServerName { get; set; }

    
    public void UpdateServerName(string? serverName)
    {
        
        ServerName = serverName;
        
        // 一些特殊的先直接过滤
        if (string.IsNullOrEmpty(Desc) || !Desc.Contains("默认"))
        {
            return;
        }
        
        if (serverName.Contains("萌新聚集地")) {}
    }
    
    // 关联
    // 可空类型表示外键关联不是必须的
    public int? TagId { get; set; }
    // [JsonIgnore] // 这个只能是临时解决办法, 我就是要双向查询
    public Tag? Tag { get; set; } 
    

    public FavoriteServer()
    {
        
    }

    // 导入json实现迁移
    public FavoriteServer(FavoriteServerJson json)
    {
        Host = json.Host;
        Port = json.Port;
        Desc = json.Desc;
        
    }


    public FavoriteServer(AddServerRequest request)
    {
        int lastIndexOf = request.Addr.LastIndexOf(":", StringComparison.Ordinal);
        if (lastIndexOf == -1)
        {
            throw new Exception("无效的Addr: 没有冒号");
        }
        Host = request.Addr.Substring(0, lastIndexOf);
        Port = int.Parse(request.Addr.Substring(lastIndexOf + 1));
        
        Desc = request.Desc;
    }
    public FavoriteServer(string host, int port)
    {
        Host = host;
        Port = port;
        CreateAt = DateTime.Now;
    }
    
    public FavoriteServer(string host, int port, string? desc)
    {
        Host = host;
        Port = port;
        CreateAt = DateTime.Now;
        Desc = desc;
    }

    public override string ToString()
    {
        return $"{Id}: \"{Host}:{Port}\" ({Desc})";
    }
}