namespace L4d2ServerQuery.Helper;

/// <summary>
/// 用于从json文件中导入和导出服务器列表, efcore文档的迁移我没看懂
/// </summary>
public static class BackupHelper
{
    public static string FilePath = "servers.json";
    
    public static void Import()
    {
        string readAllText = File.ReadAllText(FilePath);
        
    }
    
    public static void Export()
    {
        
    }
}

public class ServerIntoJson
{
    
}