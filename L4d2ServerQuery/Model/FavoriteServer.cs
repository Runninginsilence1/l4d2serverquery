using Microsoft.EntityFrameworkCore;

namespace L4d2ServerQuery.Model;

public class FavoriteServerContext : DbContext
{
    public DbSet<FavoriteServer> FavoriteServers { get; set; }

    private string DbPath { get; }

    public FavoriteServerContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "db.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
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
    
    
}