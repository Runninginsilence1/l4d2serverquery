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


