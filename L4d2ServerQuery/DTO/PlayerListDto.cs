using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SteamQuery.Models;

namespace L4d2ServerQuery.DTO;

public class PlayerListDto
{
    
    public int Id { get; set; } // 主键按照约定, 名字一般要叫 classNameId 或者 直接 Id
    public string Name { get; set; }
    [JsonIgnore]
    public TimeSpan TimeSpan { get; set; }

    public string Seconds => TimeSpan.ToString();
    
    public PlayerListDto(SteamQueryPlayer player)
    {
        Name = player.Name;
        TimeSpan = player.DurationTimeSpan;
    }
}