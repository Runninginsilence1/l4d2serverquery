namespace L4d2ServerQuery.Request;

public class TagRequest(string name, int rankSort)
{
    public string Name { get; set; } = name;
    
    public int RankSort { get; set; } // 排序, 越靠近这个值, 排名越高
}