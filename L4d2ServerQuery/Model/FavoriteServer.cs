using System.ComponentModel.DataAnnotations;
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
public class FavoriteServerContext : DbContext
{
    
    public DbSet<FavoriteServer> FavoriteServers { get; set; }

    private string DbPath { get; }

    public FavoriteServerContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "db.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    // 重写下面的方法, 使得EF可以创建SQLite数据库文件
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

// 数据库实体


public class FavoriteServer
{
    
    public int Id { get; set; } // 主键按照约定, 名字一般要叫 classNameId 或者 直接 Id
    // public int ServerId { get; set; } // 或者通过 Key 属性来指定主键
    public DateTime CreateAt { get; set; } = DateTime.Now;
    
    [Required]
    public string Host { get; set; }
    [Required]
    public int Port { get; set; }
    
    [MaxLength(255)]
    [Required]
    public string? Desc { get; set; }

    public string Addr => $"{Host}:{Port}";

    public FavoriteServer()
    {
        
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