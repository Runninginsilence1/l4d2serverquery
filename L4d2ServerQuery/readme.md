# 用于求生服务器信息查询的基于ASP.NET的最小API

我很喜欢CSharp这门语言, 同时这个功能也是我现在所需要的. 本着边学习边实践来写这个项目.

# Features(v1.0)
- 实时查询服务器列表的信息, 不需要手动点击什么刷新按钮什么的来实现
- 根据指定的结果进行排序和过滤, 这个是用于检索我需要的服务器的功能
- 针对服务器列表的CRUD, 这个是单纯的提供接口来增加或者屏蔽服务器

# 开发指南
全部都是基于 .NET 的官方技术栈, 数据库使用SQLite, Web 使用最小API. 
平台基于DOTNET8.

## 数据库设计

一张表即可.

注意自动迁移的命令:
```shell
dotnet ef migrations add InitialCreate
dotnet ef database update
```


## 问题
配置文件问题

那么前端访问的地址:



## 部署
dotnet publish --configuration Release
在你执行该目录的位置的指定位置的
.\bin\Release\net8.0\publish

指定启动的url
dotnet L4d2ServerQuery.dll --urls "http://*:6000"


前端打包并把 dist 文件放上去

/etc/nginx/nginx.conf

# 迭代
后端实时上传；通过task实现；
后端使用MSTest框架进行单元测试

前端：打包部署一气呵成；

死妈华为云找个防火墙找半天