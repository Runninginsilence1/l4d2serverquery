using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace L4d2ServerQuery.Request;

public class AddServerRequest
{
    public string? Desc { get; set; }
    [Required]
    public string Addr { get; set; }
    
    public List<int> Tags { get; set; }
}