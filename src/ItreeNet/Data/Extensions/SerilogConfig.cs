using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;

namespace ItreeNet.Data.Extensions
{
    public static class SerilogConfig
    {
        /// <summary>
        /// Configure and Create logger from appsettings
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configSettings">Configuration settings</param>
        public static void AddSerilogServices(
            IServiceCollection service,
            IConfiguration configSettings)
        {

            // Should be only null if its a test
            if (configSettings.GetChildren().Any(x => x.Key == "Serilog"))
            {
                var connectionString = configSettings.GetConnectionString("APP");
                var table = configSettings["Serilog:Table"];
                var levelSwitch = new LoggingLevelSwitch(LogLevel(configSettings["Serilog:MinimumLevel"]!));
                var createTable = bool.Parse(configSettings["Serilog:autoCreateSqlTable"]!);

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .WriteTo.PostgreSQL(
                        connectionString: connectionString,
                        tableName: table,
                        schemaName: "dbo",
                        needAutoCreateTable: createTable
                    )
                    .WriteTo.Console()
                    .CreateLogger();

                Log.Warning("Application started");
                service.AddSingleton(Log.Logger);
                service.AddSingleton(levelSwitch);
            }
        }

        /// <summary>
        /// Convert string to Loglevel. Default = Warning
        /// </summary>
        /// <param name="logLevelString"></param>
        /// <returns></returns>
        public static LogEventLevel LogLevel(string logLevelString)
        {
            return logLevelString switch
            {
                "Verbose" => LogEventLevel.Verbose,
                "Debug" => LogEventLevel.Debug,
                "Information" => LogEventLevel.Information,
                "Warning" => LogEventLevel.Warning,
                "Error" => LogEventLevel.Error,
                "Fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Warning
            };
        }
    }
}
