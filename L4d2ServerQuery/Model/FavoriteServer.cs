using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace L4d2ServerQuery.Model;

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
    
    public int ServerId { get; set; }
    public DateTime CreateAt { get; set; }
    
    public string Host { get; set; }
    public int Port { get; set; }
    
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
}