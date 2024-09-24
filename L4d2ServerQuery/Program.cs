using System.Buffers;
using System.Diagnostics;
using L4d2ServerQuery;
using L4d2ServerQuery.Data;
using L4d2ServerQuery.Model;
using L4d2ServerQuery.Service;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SteamQuery;
using SteamQuery.Exceptions;


var builder = WebApplication.CreateBuilder(args);

Init();


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

    // 最好不要在这里迁移
    // await context.Database.MigrateAsync();

    // 而是检查有没有待迁移的内容
    var pendingMigrations = context.Database.GetPendingMigrations();
    var migrations = pendingMigrations as string[] ?? pendingMigrations.ToArray();
    if (migrations.Any())
    {
        // throw new Exception("存在没有迁移的数据库");
        Log.Error("存在没有迁移的数据库:");
        foreach (string migration in migrations)
        {
            Log.Warning($"{migration} 未迁移");
        }
        Environment.Exit(1);
    }
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


{
    app.MapGet("/getAllTags", (ServerContext db) => db.Tags.ToList())
        .WithName("GetAllTags")
        .WithOpenApi();

    app.MapPost("/addTag", async (Tag tag, ServerContext db) =>
        {
            tag.CreateAt = DateTime.Now;
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
            return Results.Ok();
        })
        .WithName("AddTag")
        .WithOpenApi();

// 标准的删除步骤
// 删除tag时将所有包含有该tag的server的tag也删除
// efcore 给出的示例是， 直接将对应的导航字段设置为 null, 然后保存即可
    app.MapDelete("/deleteTag/{id}", async (int id, ServerContext db) =>
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
        catch (InvalidOperationException e)
        {
            return Results.NotFound();
        }
    });
}


app.MapGet("/favoriteServers", (ServerContext db) =>
    {
        var res =db.FavoriteServers.Include(s => s.Tag).ToList();
        return Results.Ok(res);
    })
    .WithName("FavoriteServers")
    .WithOpenApi();

// 似乎直接在 lambda 表达式中指定 参数就可以了
// 可选：指定tag的id
app.MapPost("/serverAdd", async (FavoriteServer server, ServerContext db) =>
    {
        Tag tag;
        try
        {
            tag = db.Tags.Single(s => s.Id == server.TagId);
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound();
        }
        
        
        server.CreateAt = DateTime.Now;
        server.Tag = tag; 
        db.FavoriteServers.Add(server);
        await db.SaveChangesAsync();
        Log.Information($"添加了新的服务器, 当前服务器数量为: {db.FavoriteServers.Count()}");
        return Results.Ok();
    })
    .WithName("ServerAdd")
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
    catch (InvalidOperationException e)
    {
        return Results.NotFound();
    }
});

var stopwatch = new Stopwatch();

app.MapGet("/serverList/{id}", async (int? id, ServerContext db) =>
    {
        List<FavoriteServer> servers;

        if (id == null)
        {
            servers = db.FavoriteServers.ToList();
        }
        else
        {
            // 可能会有没有找到的异常
            var single = db.Tags.Single(t => t.Id == id);
            servers = single.Servers.ToList();
        }

        var status = new List<ServerStatus>();
        var tasks = new List<Task>();
        var count = 0;


        stopwatch.Restart();

        foreach (var server in servers)
        {
            var host = server.Addr;

            GameServer gameServer;

            try
            {
                gameServer = new GameServer(host)
                {
                    SendTimeout = 1000,
                    ReceiveTimeout = 1000,
                };
            }
            catch (AddressNotFoundException e)
            {
                Console.WriteLine($"{host} 是一个无效的地址");
                continue;
            }


            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var info = await gameServer.GetInformationAsync();
                    count++;
                    var s = new ServerStatus()
                    {
                        Id = server.Id,
                        Address = host,
                        ServerName = info.ServerName,
                        Map = info.Map,
                        OnlinePlayers = info.OnlinePlayers,
                        MaxPlayers = info.MaxPlayers,
                    };

                    status.Add(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{host} 无法连接");
                }
            }));
        }

        await Task.WhenAll(tasks);

        const int expectedPlayers = 8;

        var result = status.OrderBy(s => Math.Abs(s.OnlinePlayers - expectedPlayers)).ToList();
        stopwatch.Stop();
        Console.WriteLine($"查询了 {count} 个服务器, 用时: {stopwatch.ElapsedMilliseconds} ms");
        return Results.Ok(result);
    })
    .WithName("ServerList")
    .WithOpenApi();

{
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    var DbPath = Path.Join(path, "db.db");
    Log.Information($"数据库的路径在: {DbPath}");
}


app.Run();


void Init()
{
    Console.WriteLine("调用了Init");
    MyLogger.Init();
}