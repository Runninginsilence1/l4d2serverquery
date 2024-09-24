using L4d2ServerQuery.Model;
using SteamQuery;
using SteamQuery.Models;

namespace L4d2ServerQuery.Service;

// 思路是: 维护一个自己有定时器来执行查询的类列表.
// 这样我只要获取List, 就能获取到所有的实时的服务器信息.

// 这个类里面的数据库查询是可以正常查找到数据的;
