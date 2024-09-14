using L4d2ServerQuery.Model;
using L4d2ServerQuery.Service;
using SteamQuery;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder
builder.Services.AddDbContext<FavoriteServerContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 全局变量
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        // Enumerable.Range(1, 5) 用于生成1到5的数字序列, 特别好用

        // Select 使用 Linq 语法, 用于将序列中的每个元素映射到一个新值, 这里我们生成了5个 WeatherForecast 对象
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)), // 用索引生成日期
                    Random.Shared.Next(-20, 55), // 生成随机数
                    summaries[Random.Shared.Next(summaries.Length)] // 在数组内随机取一个值作为结果
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/hahaha", () => summaries[Random.Shared.Next(summaries.Length)])
    .WithName("hahaha")
    .WithOpenApi();

// 上面是测试用api
// 下面是 服务器查询相关的api

var serverQueryService = new ServerQueryService();
serverQueryService.UpdateServers();


// 获取服务器信息
// 
app.MapGet("/test", () =>
    {
    
        var res = serverQueryService.Servers.ToList();
        return res;
    })
    .WithName("Test")
    .WithOpenApi();


// CRUD api
app.MapGet("/favoriteServers", (FavoriteServerContext db) => db.FavoriteServers.ToList())
    .WithName("FavoriteServers")
    .WithOpenApi();

// 似乎直接在 lambda 表达式中指定参数就可以了
app.MapPost("/serverAdd", async (FavoriteServer server, FavoriteServerContext db) =>
    {
        db.FavoriteServers.Add(server);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithName("ServerAdd")
    .WithOpenApi();

app.MapGet("/serverList", async (FavoriteServerContext db) =>
    {
        var servers = db.FavoriteServers.ToList();
        var status = new List<ServerStatus>();
        
            foreach (var server in servers)
            {

                var host = server.Addr;
                var gameServer = new GameServer(host)
                {
                    SendTimeout = 1000,
                    ReceiveTimeout = 1000,
                };
                
                try
                {
                    var info = await gameServer.GetInformationAsync();
                    var s = new ServerStatus()
                    {
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
                
                

                

            }
            
            return Results.Ok(status);
        
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


