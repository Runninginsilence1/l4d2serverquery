using System.ComponentModel.DataAnnotations.Schema;

namespace L4d2ServerQuery.Model;


public class JsonServer
{
/// <summary>
/// 
/// </summary>
public int Id { get; set; }
/// <summary>
/// 
/// </summary>
public string CreateAt { get; set; }
/// <summary>
/// 
/// </summary>
public string Host { get; set; }
/// <summary>
/// 
/// </summary>
public int Port { get; set; }
/// <summary>
/// 本地错误, 看是否可以获取异常
/// </summary>
public string Desc { get; set; }
}