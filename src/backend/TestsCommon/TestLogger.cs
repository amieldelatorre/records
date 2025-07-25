namespace TestsCommon;

public static class TestLogger
{
    public static Serilog.ILogger GetLogger()
    {
        return Serilog.Log.Logger = new Serilog.LoggerConfiguration()
            .MinimumLevel.Error()
            .CreateLogger();
    }
}