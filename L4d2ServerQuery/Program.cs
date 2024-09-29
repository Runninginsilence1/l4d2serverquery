using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using L4d2ServerQuery;
using L4d2ServerQuery.Data;
using L4d2ServerQuery.Model;
using L4d2ServerQuery.Request;
using L4d2ServerQuery.Service;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SteamQuery;
using SteamQuery.Exceptions;
using TagList = L4d2ServerQuery.Request.TagList;


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
// builder.Services.AddDbContext<TagContext>();
// builder.Services.AddDbContext<ExcludedServerContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
// handle db context

    var context = scope.ServiceProvider.GetRequiredService<ServerContext>();

    
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

    // {
    //     context.Database.EnsureDeleted();
    //     context.Database.EnsureCreated();    
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
    app.MapGet("/tags", (ServerContext db) => db.Tags.Select(t => new {t.Id, t.Name}))
        .WithName("获取所有标签")
        .WithOpenApi();
    
    app.MapPost("/tags/add", async (Tag tag, ServerContext db) =>
        {
            tag.CreateAt = DateTime.Now;
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
            return Results.Ok();
        })
        .WithName("新增标签")
        .WithOpenApi();
    
    // 这个实际上是新增服务器的时候应该绑定的, 写在tag的api这里了
    // 这个没有api文档
    app.MapPost("/tag/add", (TagList tagList) =>
    {
        IEnumerable<Tag> tags = tagList.Tags.Select(t => new Tag(t));
        Log.Information("尝试解析Tag");
        string serialize = JsonSerializer.Serialize(tags);
        Console.WriteLine(serialize);
    });
    
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
app.MapGet("/servers", (ServerContext db) =>
    {
        var res = db.FavoriteServers.Include(s => s.Tag).ToList();


        // 直接适用关键的方式来查询
        // var query = from s in db.FavoriteServers
        //     join t in db.Tags on s.TagId equals t.Id
        //     select new {s, t};
        //
        // var res = query.ToList();
        
        return Results.Ok(res);
    })
    .WithName("查询所有服务器")
    .WithOpenApi();

// 似乎直接在 lambda 表达式中指定 参数就可以了
// 可选：指定tag的id
app.MapPost("/servers/add", async (AddServerRequest request, ServerContext db) =>
    {
        var server = new FavoriteServer(request);
        Log.Information($"待添加的服务器信息: {server}");

        server.CreateAt = DateTime.Now;
        Tag tag;
        try
        {
            tag = db.Tags.Single(s => s.Id == server.TagId);
            tag.Servers.Add(server);
        }
        catch (InvalidOperationException e)
        {
            Log.Information($"{server.Addr}没有添加tag, 默认直接添加");
            db.FavoriteServers.Add(server);
        }
        await db.SaveChangesAsync();
        Log.Information($"添加了新的服务器, 当前服务器数量为: {db.FavoriteServers.Count()}");
        return Results.Ok();
    })
    .WithName("增加服务器")
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

var stopwatch = new Stopwatch();

app.MapGet("/serverList/{id}", async (int? id, ServerContext db) =>
    {
        List<FavoriteServer> servers;

        if (id is null or 0)
        {
            servers = db.FavoriteServers.ToList();
        }
        else
        {
            // 可能会有没有找到的异常
            try
            {
                var single = db.Tags.Single(t => t.Id == id);
                servers = single.Servers.ToList();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
                return Results.NotFound();
            }
        }

        var result = await QueryService.Query(servers);
        return Results.Ok(result);
    })
    .WithName("查询服务器列表")
    .WithOpenApi();

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
    Log.Information($"数据库的路径在: {dbPath}");   
}