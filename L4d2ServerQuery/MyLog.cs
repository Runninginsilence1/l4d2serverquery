


namespace L4d2ServerQuery;

using Serilog;

public static class MyLogger
{
    public static void Init()
    {
        var log = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger();

        Log.Logger = log;
        
    }
}