using System.Buffers;
using L4d2ServerQuery.Model;
using L4d2ServerQuery.Service;
using SteamQuery;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<FavoriteServerContext>();
builder.Services.AddDbContext<TagContext>();
// builder.Services.AddDbContext<ExcludedServerContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");



// CRUD api

app.MapGet("/getAllTags", (TagContext db) => db.Tags.ToList())
    .WithName("GetAllTags")
    .WithOpenApi();

app.MapPost("/addTag", async (Tag tag, TagContext db) =>
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
app.MapDelete("/deleteTag/{id}", async (int id, TagContext db) =>
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


app.MapGet("/favoriteServers", (FavoriteServerContext db) => db.FavoriteServers.ToList())
    .WithName("FavoriteServers")
    .WithOpenApi();

// 似乎直接在 lambda 表达式中指定参数就可以了
// 可选：指定tag的id
app.MapPost("/serverAdd", async (FavoriteServer server, FavoriteServerContext db) =>
    {
        server.CreateAt = DateTime.Now;
        db.FavoriteServers.Add(server);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithName("ServerAdd")
    .WithOpenApi();

// 标准的删除步骤
app.MapDelete("/serverDelete/{id}", async (int id, FavoriteServerContext db) =>
{
    // 从数据库中查询: 查询单个数据使用 single, 使用 lambda 表达式来指定条件
    // 注意处理 NotFound异常
    try
    {
        FavoriteServer server = db.FavoriteServers.Single(s => s.Id == id);
        db.Remove(server);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (InvalidOperationException e)
    {
        return Results.NotFound();
    }
    
});

app.MapGet("/serverList/{id}", async (int? id, TagContext db, FavoriteServerContext fdb) =>
    {
        List<FavoriteServer> servers;
        
        if (id == null)
        {
            servers = fdb.FavoriteServers.ToList();   
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

        foreach (var server in servers)
        {
            var host = server.Addr;
            var gameServer = new GameServer(host)
            {
                SendTimeout = 1000,
                ReceiveTimeout = 1000,
            };

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
        return Results.Ok(result);
    })
    .WithName("ServerList")
    .WithOpenApi();

{
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    var DbPath = Path.Join(path, "db.db");
    Console.WriteLine($"数据库的路径在: {DbPath}");
}


app.Run();