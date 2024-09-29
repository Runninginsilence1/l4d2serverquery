using System.ComponentModel.DataAnnotations;

namespace L4d2ServerQuery.Request;

public class AddServerRequest
{
    public string? Desc { get; set; }
    [Required]
    public string Addr { get; set; }
}