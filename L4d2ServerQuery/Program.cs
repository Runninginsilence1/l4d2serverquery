using System.Text.Json.Serialization;
using L4d2ServerQuery;
using L4d2ServerQuery.Data;
using L4d2ServerQuery.Model;
using L4d2ServerQuery.Request;
using L4d2ServerQuery.Service;
using Microsoft.EntityFrameworkCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

Init();

// 尝试解决 json 的循环引用的问题
// 我总觉得这个玩意没有生效
// builder.Services.AddControllers()
//     .AddJsonOptions(option => 
//         option.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


// 可能上面那种方法不在 最小化api上适用
// 使用这种配置成功!
builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }
);

// 添加 CORS 服务
builder.Services.AddCors(options =>
{
    // 允许所有来源
    options.AddPolicy(name: "AllowAllOrigins",
        policyBuilder => policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// efcore context register

// 操, 我还以为是一个 context 和一个数据库模型绑定

builder.Services.AddDbContext<ServerContext>();

var app = builder.Build();


// 数据库迁移相关的代码
using (var scope = app.Services.CreateScope())
{
// handle db context

    var context = scope.ServiceProvider.GetRequiredService<ServerContext>();
    
    
    
    // 这里用于处理数据库的自动更新

    int queryNum = 0;
    try
    {
        var queryNum1 = context.FavoriteServers.Count();
        var queryNum2 = context.Tags.Count();
        
        queryNum = queryNum1 * queryNum2;
        
        // 如果数据库中没有任何数据, 则尝试导入数据
    } catch (Exception e)
    {
        // Log.Error($"查询时出现异常:\n{e.Message}");
        // {
        //     Log.Warning("清除了数据库");
        //     context.Database.EnsureDeleted();
        //     context.Database.EnsureCreated();    
        // }
        //
    }

    if (queryNum == 0)
    {
        Log.Error($"重新创建数据库");
        {
            Log.Warning("清除了数据库");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();    
        }

        
        Log.Warning("数据库中没有任何数据");
        Log.Information("尝试导入数据");
        DataTransformHighway.Import(Path.Join(GetLocalAppDataPath(), "data.json"));
        Log.Information("导入数据成功");
    }
    else
    {
        Log.Information($"数据库中有 {queryNum} 条数据");
    }

    
    // 使用代码完成迁移
    {
        // 最好不要在这里迁移
        // await context.Database.MigrateAsync();
    
    }

    // 而是检查有没有待迁移的内容
    // {
    //     var pendingMigrations = context.Database.GetPendingMigrations();
    //     var migrations = pendingMigrations as string[] ?? pendingMigrations.ToArray();
    //     if (migrations.Any())
    //     {
    //         throw new Exception("存在没有迁移的数据库");
    //         // Log.Error("存在没有迁移的数据库:");
    //         // foreach (string migration in migrations)
    //         // {
    //         //     Log.Warning($"{migration} 未迁移");
    //         // }
    //         // Environment.Exit(1);
    //     }    
    // }

    
    // 自动重建然后删除表
    

}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");


// CRUD api

// tag api

// 使用 restful api 风格的路由
{
    app.MapGet("/tags", (ServerContext db) => db.Tags.Select(t => new {t.Id, t.Name, t.Servers}))
        .WithName("获取所有标签")
        .WithOpenApi();
    
    app.MapPost("/tags/add", async (TagRequest request, ServerContext db) =>
        {
            var tag = new Tag(request.Name)
            {
                CreateAt = DateTime.Now,
                // Name = request.Name,
                RankSort = request.RankSort
            };
            
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
            return Results.Ok();
        })
        .WithName("新增一个标签")
        .WithOpenApi();
    
    // 这个实际上是新增服务器的时候应该绑定的, 写在tag的api这里了
    // 这个没有api文档
    app.MapPost("/tag/addList", (QueryOption tagList) =>
    {
        // IEnumerable<Tag> tags = tagList.Tags.Select(t => new Tag(t));
        // Log.Information("尝试解析Tag");
        // string serialize = JsonSerializer.Serialize(tags);
        // Console.WriteLine(serialize);
    }).WithName("新增多个标签")
    .WithOpenApi();
    
    app.MapDelete("/deleteTag/{id:int}", async (int id, ServerContext db) =>
    {
        // 从数据库中查询: 查询单个数据使用 single, 使用 lambda 表达式来指定条件
        // 注意处理 NotFound异常
        try
        {
            Tag tag = db.Tags.Single(s => s.Id == id);

            // 先删除所有与该tag相关的server的tag
            // 请你把代码加在这里， 获取 tag 绑定的服务器是 tag.Servers

            tag.Servers.Clear(); // 直接通过 clear 断开联系

            db.Remove(tag);
            await db.SaveChangesAsync();
            return Results.Ok();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    });    

}

// 测试接口, 显示所有的服务器数据
app.MapGet("/debug/allServers", (ServerContext db) =>
    {
        var res = db.FavoriteServers
            // .Include(s => s.Tag)
            .ToList();
        
        // 直接适用关键的方式来查询
        // var query = from s in db.FavoriteServers
        //     join t in db.Tags on s.TagId equals t.Id
        //     select new {s, t};
        //
        // var res = query.ToList();
        var res1 = new { Count = res.Count, Servers = res };
        
        
        return Results.Ok(res1);
    })
    .WithName("查询所有服务器")
    .WithOpenApi();

// 测试接口, 基于 DistinctBy 去重
// Expect api用于清除重复服务器
app.MapGet("/debug/cleanServers", async (ServerContext db) =>
    {
        // 查询所有服务器
        var allServers = await db.FavoriteServers.ToListAsync();

        // 使用 DistinctBy 方法去除重复的服务器
        var newServers = allServers.DistinctBy(s => s.Addr).ToList();

        // 删除数据库中未在去重列表中的服务器
        var serversToRemove = allServers.Except(newServers).ToList();
        foreach (var server in serversToRemove)
        {
            db.FavoriteServers.Remove(server);
        }

        // 保存更改
        await db.SaveChangesAsync();

        return Results.Ok(new { Message = "重复服务器已去除", Count = serversToRemove.Count });
    })
    .WithName("去除重复服务器")
    .WithOpenApi();
// 似乎直接在 lambda 表达式中指定 参数就可以了
// 可选：指定tag的id
// 别管tag了
// 弄一个no tag 版本
app.MapPost("/servers/add", async (AddServerRequest request, ServerContext db) =>
    {
        // 捕获初始化异常
        FavoriteServer server;
        try
        {
            server = new FavoriteServer(request);
        }
        catch (Exception e)
        {
            Log.Debug($"Request: {request}");
            Log.Error("[DEBUG]初始化异常");
            Log.Error(e, "异常信息");
            throw;
        }
        
        Log.Information($"待添加的服务器信息: {server}");

        server.CreateAt = DateTime.Now;
        
        // 不处理分类
        db.FavoriteServers.Add(server);
        
        // 如果处理分类
        // foreach (int tagId in request.Tags)
        // {
        //     try
        //     {
        //         Tag tag;
        //         tag = db.Tags.Single(s => s.Id == tagId);
        //         tag.Servers.Add(server);
        //     }
        //     catch (InvalidOperationException)
        //     {
        //         Log.Information($"{server.Addr}没有添加tag, 默认直接添加");
        //         db.FavoriteServers.Add(server);
        //     }
        //     catch (Exception e)
        //     {
        //         Log.Error($"出现了未知的异常: {e.Message}");
        //         throw;
        //     }
        //
        // }
        
        await db.SaveChangesAsync();
        Log.Information($"添加了新的服务器, 当前服务器数量为: {db.FavoriteServers.Count()}");
        return Results.Ok();
    })
    .WithName("增加服务器")
    .WithOpenApi();

// 更新服务器的数据
// 现在就把他的数据更新最新时间即可

app.MapGet("/lastCopyTimeUpdate/{id}", async (int id, ServerContext db) =>
    {
        // 从数据库中查询: 查询单个数据使用 single, 使用 lambda 表达式来指定条件
        // 注意处理 NotFound异常
        try
        {
            FavoriteServer server = db.FavoriteServers.Single(s => s.Id == id);
            server.LastQueryAt = DateTime.Now;
            await db.SaveChangesAsync();
            return Results.Ok();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    })
    .WithName("更新最后复制时间")
    .WithOpenApi();

// 标准的删除步骤
app.MapDelete("/serverDelete/{id}", async (int id, ServerContext db) =>
{
    // 从数据库中查询: 查询单个数据使用 single, 使用 lambda 表达式来指定条件
    // 注意处理 NotFound异常
    try
    {
        FavoriteServer server = db.FavoriteServers.Single(s => s.Id == id);
        db.Remove(server);
        await db.SaveChangesAsync();
        Log.Information($"删除了新的服务器, 当前服务器数量为: {db.FavoriteServers.Count()}");
        return Results.Ok();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
})
.WithName("删除服务器");


app.MapGet("/serverList", async (ServerContext db) =>
    {
        List<FavoriteServer> servers;
        
        servers = db.FavoriteServers.ToList(); // 直接获取所有的服务器

        // 根据传输的id来获取
        // if (id is null or 0)
        // {
        //     servers = db.FavoriteServers.ToList();
        // }
        // else
        // {
        //     // 可能会有没有找到的异常
        //     try
        //     {
        //         var single = db.Tags.Single(t => t.Id == id);
        //         servers = single.Servers.ToList();
        //     }
        //     catch (InvalidOperationException e)
        //     {
        //         Console.WriteLine(e);
        //         return Results.NotFound();
        //     }
        // }

        var result = await QueryService.Query(servers);

        
        // result是需要返回的
        // todo: 这里跑一次就行了，记得注释掉

        // {
        // int total = result.Count;
        // int counter = 0;

        //     result.ForEach(dto =>
        //     {
        //     
        //     
        //         var rawServer = db.FavoriteServers.Single(s1 => s1.Id == dto.Id);
        //         var tags = db.Tags.ToList();
        //         foreach (var tag in tags)
        //         {
        //
        //             if (dto.ServerName == null)
        //             {
        //                 continue;
        //             }
        //         
        //             if (dto.ServerName.Contains(tag.Name))
        //             {
        //                 counter++;
        //                 tag.Servers.Add(rawServer);
        //                 Log.Information($"将{dto.ServerName}添加到{tag.Name}标签中");
        //                 break;
        //             }
        //         }
        //     
        //     
        //     });
        //
        //     Log.Information($"成功处理了{counter}个服务器。, 仍有{total - counter}个服务器未处理");
        //     db.SaveChanges();   
        // }
        
        

        
        return Results.Ok(result);
    })
    .WithName("查询服务器列表")
    .WithOpenApi();

// v2要传输tag列表
// 可以传输分类列表
// 可以传入
app.MapPost("/serverList/v2", async (QueryOption option, ServerContext db) =>
    {
        List<FavoriteServer> servers = new List<FavoriteServer>();
        foreach (var id in option.Tags)
        {
            
            // 根据传输的id来获取
            if (id is 0)
            {
                // servers = db.FavoriteServers.ToList();
            }
            else
            {
                // 可能会有没有找到的异常
                try
                {
                    var single = db.Tags.
                        Include(s => s.Servers).
                        Single(t => t.Id == id);
                    servers.AddRange(single.Servers);
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e);
                    continue;
                    // return Results.NotFound();
                }
            }
        }
        
        Log.Information($"查询了{servers.Count}个服务器");

        if (servers.Count == 0)
        {
            servers = db.FavoriteServers.ToList(); // 直接获取所有的服务器            
        }

        List<ServerStatusDto> result;
        if (option.Rank is not null)
        {
            var rank = option.Rank?? 8;
            result = await QueryService.Query(servers, rank);
        }
        else
        {
            result = await QueryService.Query(servers);
        }
        

        
        // result是需要返回的
        // todo: 这里跑一次就行了，记得注释掉

        
        
        

        
        return Results.Ok(result);
    })
    .WithName("查询服务器列表v2")
    .WithOpenApi();

// 自动校对服务器与tag的连接
// 通过 serverName 与服务器的 Tag 字段相互匹配
app.MapGet("/debug/fixTag", async (ServerContext db) =>
{
    
    List<FavoriteServer> servers;
        
    servers = db.FavoriteServers.ToList(); // 直接获取所有的服务器

    var result = await QueryService.Query(servers);
    {
        int total = result.Count;
        int counter = 0;

        result.ForEach(dto =>
        {


            var rawServer = db.FavoriteServers.Single(s1 => s1.Id == dto.Id);
            var tags = db.Tags.ToList();
            foreach (var tag in tags)
            {

                if (dto.ServerName == null)
                {
                    continue;
                }

                if (dto.ServerName.Contains(tag.Name))
                {
                    counter++;
                    tag.Servers.Add(rawServer);
                    Log.Information($"将{dto.ServerName}添加到{tag.Name}标签中");
                    break;
                }
            }
        });

        Log.Information($"成功处理了{counter}个服务器。, 仍有{total - counter}个服务器未处理");
        db.SaveChanges();
    }
});

// // 这个备用留着
// app.MapGet("/serverList/{id}", async (int? id, ServerContext db) =>
//     {
//         List<FavoriteServer> servers;
//
//         if (id is null or 0)
//         {
//             servers = db.FavoriteServers.ToList();
//         }
//         else
//         {
//             // 可能会有没有找到的异常
//             try
//             {
//                 var single = db.Tags.Single(t => t.Id == id);
//                 servers = single.Servers.ToList();
//             }
//             catch (InvalidOperationException e)
//             {
//                 Console.WriteLine(e);
//                 return Results.NotFound();
//             }
//         }
//
//         var result = await QueryService.Query(servers);
//         return Results.Ok(result);
//     })
//     .WithName("查询服务器列表WithTagId")
//     .WithOpenApi();

// 根据ip查询玩家的名字的接口, 这个因为前端的原因所以没写
app.MapGet("/playerList/{id:int}", async (int id, ServerContext db) =>
    {
        // 从数据库中查询: 查询单个数据使用 single, 使用 lambda 表达式来指定条件
        // 注意处理 NotFound异常
        try
        {
            FavoriteServer server = db.FavoriteServers.Single(s => s.Id == id);
            return Results.Ok(await QueryService.QueryPlayers(server.Addr));
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    })
    .WithName("查询服务器中的玩家信息");


// 判断db是否有数据: 

const string buildTimeString = "2022-01-10 16:00:00";

Log.Information($"最后编译时间: {buildTimeString}");

app.Run();


void Init()
{
    Console.WriteLine("调用了Init");
    MyLogger.Init();
    
    PrintDbPath();
}

void PrintDbPath()
{
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    var dbPath = Path.Join(path, "db.db");
    // C:\Users\zzk\AppData\Local
    Log.Information($"数据库的路径在: {dbPath}");   
}

string GetLocalAppDataPath()
{
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    return path;
}