using System.IO;
using System.Linq;
using Bot.Application.ChatAi.OpenRouter;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;

namespace Bot.Host;

internal static class LoggingConfigurator
{
    private const string Folder = "logs";

    private static readonly string[] SpecialLoggers =
    [
        nameof(ChatAiOpenRouteLogger),
        "Microsoft.EntityFrameworkCore.Database.Command"
    ];

    internal static ILoggerFactory CreateLoggerFactory()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            
            .WriteFiltered("general",  excludeFilters: SpecialLoggers)
            
            .WriteFiltered("open-router-requests", includeFilters: [nameof(ChatAiOpenRouteLogger)])
            .WriteFiltered(
                "sql-requests",
                includeFilters: ["Microsoft.EntityFrameworkCore.Database.Command"],
                formatter: new SingleLineSqlFormatter()
            )

            .CreateLogger();

        return LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
    }

    private static LoggerConfiguration WriteFiltered(
        this LoggerConfiguration logger,
        string fileName,
        string[]? includeFilters = null,
        string[]? excludeFilters = null,
        RollingInterval rollingInterval = RollingInterval.Day,
        ITextFormatter? formatter = null)
    {
        return logger.WriteTo.Logger(lc =>
        {
            if (includeFilters is { Length: > 0 })
            {
                lc = lc.Filter.ByIncludingOnly(evt => includeFilters.Any(f => Filter(evt, f)));
            }

            if (excludeFilters is { Length: > 0 })
            {
                lc = lc.Filter.ByExcluding(evt => excludeFilters.Any(f => Filter(evt, f)));
            }
            
            if (formatter != null)
            {
                string path = $"{Folder}/{fileName}-.log";
                lc = lc.WriteTo.File(formatter, path, rollingInterval: rollingInterval);
            }
            else
            {
                lc = lc.WriteToFile(fileName, rollingInterval);
            }

            lc.WriteTo.Console();
        });
    }

    private static LoggerConfiguration WriteToFile(this LoggerConfiguration logger, string fileName, RollingInterval rollingInterval)
    {
        string path = $"{Folder}/{fileName}-.log";
        return logger.WriteTo.File(
            path,
            rollingInterval: rollingInterval,
            outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}"
        );
    }

    private static bool Filter(LogEvent evt, string loggerName) =>
        evt.Properties.TryGetValue("SourceContext", out var context) &&
        context.ToString().Trim('"').Contains(loggerName);
}


class SingleLineSqlFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        string timestamp = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string level = logEvent.Level.ToString().ToUpper();

        string duration = logEvent.Properties.TryGetValue("elapsed", out var propElapsed)
            ? propElapsed.ToString()
            : "0";

        string commandText = "";
        string parameters = "";

        if (logEvent.Properties.TryGetValue("commandText", out LogEventPropertyValue? propCmd))
            commandText = propCmd.ToString().Replace("\n", " ").Replace("\r", "");

        if (logEvent.Properties.TryGetValue("parameters", out LogEventPropertyValue? propParams))
            parameters = propParams.ToString().Replace("\n", " ").Replace("\r", "");

        string sqlLine = string.IsNullOrEmpty(parameters) ? commandText : $"{commandText} -- {parameters}";
        
        output.WriteLine($"[{timestamp}] [{level}] [DURATION] {duration}ms [SQL] {sqlLine}");
    }
}
