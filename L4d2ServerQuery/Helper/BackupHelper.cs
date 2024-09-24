using System.Diagnostics.CodeAnalysis;

namespace L4d2ServerQuery.Helper;

/// <summary>
/// 用于从json文件中导入和导出服务器列表, efcore文档的迁移我没看懂
/// </summary>
public static class BackupHelper
{
    public static string FilePath = "servers.json";
    
    public static List<ImportModel> Import()
    {
        
        var importModels = new List<ImportModel>();


        if (!File.Exists(FilePath))
        {
            File.Create(FilePath).Close();
            return importModels;
        }
        var readAllLines = File.ReadAllLines(FilePath);


        foreach (var line in readAllLines)
        {
            var importModel = new ImportModel(line);
                importModels.Add(importModel);
        }

        return importModels;
    }
    
    public static void Export()
    {
        
    }
}


/// <summary>
/// 用于导出的模型
/// </summary>
public class ImportModel
{
    public ImportModel(string addr)
    {
        var lastIndexOf = addr.LastIndexOf(":", StringComparison.Ordinal);
        Host = addr.Substring(0, lastIndexOf);
        Host = addr.Substring(lastIndexOf, addr.Length);
    }

    public string Host { get; set; }
    public int Port { get; set; }
}