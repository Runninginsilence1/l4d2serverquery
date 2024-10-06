using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using L4d2ServerQuery.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace L4d2ServerQuery.Model;

public class FavoriteServerJson
{
    public int Id { get; set; } 
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? Desc { get; set; }

    public string Addr => $"{Host}:{Port}";
}