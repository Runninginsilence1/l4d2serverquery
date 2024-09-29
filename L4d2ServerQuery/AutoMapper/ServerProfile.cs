using AutoMapper;
using L4d2ServerQuery.Model;

namespace L4d2ServerQuery.AutoMapper;

// 这只是一个demo, 没有实际使用
// XXXProfile 是 AutoMapper 的 Profile 名称, 请根据实际情况修改
public class ServerProfile: Profile
{
    public ServerProfile()
    {
        CreateMap<FavoriteServer, ServerStatusDto>();
    }
}