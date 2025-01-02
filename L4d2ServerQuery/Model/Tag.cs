using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace L4d2ServerQuery.Model;

[Table("tags")]
public class Tag(string name)
{
    public int Id { get; set; } // 主键按照约定, 名字一般要叫 classNameId 或者 直接 Id
    // public int ServerId { get; set; } // 或者通过 Key 属性来指定主键
    public DateTime CreateAt { get; set; } = DateTime.Now;
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = name;
    
    public int RankSort { get; set; } // 排序, 越靠近这个值, 排名越高
    public ICollection<FavoriteServer> Servers { get; } = new List<FavoriteServer>();
}
