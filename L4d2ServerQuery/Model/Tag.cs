using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace L4d2ServerQuery.Model;

public class Tag(string name)
{
    public int Id { get; set; } // 主键按照约定, 名字一般要叫 classNameId 或者 直接 Id
    // public int ServerId { get; set; } // 或者通过 Key 属性来指定主键
    public DateTime CreateAt { get; set; } = DateTime.Now;
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = name;
    
    public ICollection<FavoriteServer> Servers { get; } = new List<FavoriteServer>();
    
    
}

public sealed class TagContext : DbContext
{
    
    public DbSet<Tag> Tags { get; set; }

    private string DbPath { get; }

    public TagContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "db.db");

        // Console.WriteLine("尝试迁移表 Tag");
        // var ensureDeleted = Database.EnsureDeleted();
        // Console.WriteLine($"Deleted: {ensureDeleted}");
        // var ensureCreated = Database.EnsureCreated();
        // Console.WriteLine($"Created: {ensureCreated}");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    // 重写下面的方法, 使得EF可以创建SQLite数据库文件
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
