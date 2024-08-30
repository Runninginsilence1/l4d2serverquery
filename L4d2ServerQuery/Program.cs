using L4d2ServerQuery.Model;
using L4d2ServerQuery.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var db = new FavoriteServerContext();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
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

// 
app.MapGet("/serverQuery", () =>
    {
        serverQueryService.UpdateServers();
        return serverQueryService.Servers.ToList();
    })
    .WithName("ServerQuery")
    .WithOpenApi();

app.MapGet("/serverAdd", () =>
    {
        serverQueryService.AddServer(new FavoriteServer() { ServerId = 1,Host = "42.192.4.35", Port = 42300, Desc = "我去, 喵都!"});
    })
    .WithName("ServerAdd")
    .WithOpenApi();

app.Run();


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}