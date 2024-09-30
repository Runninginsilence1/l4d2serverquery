using L4d2ServerQuery.Model;
using Microsoft.EntityFrameworkCore;

namespace L4d2ServerQuery.Data;

public class ServerContext: DbContext
{
    
    public DbSet<FavoriteServer> FavoriteServers { get; set; }
    public DbSet<Tag> Tags { get; set; }

    private string DbPath { get; }

    public ServerContext()
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