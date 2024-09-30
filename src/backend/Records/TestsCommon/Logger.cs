namespace Common;

public static class Logger
{
   public static Serilog.ILogger GetLogger()
   {
      return Serilog.Log.Logger = new Serilog.LoggerConfiguration()
         .MinimumLevel.Error()
         .CreateLogger();
   }
}