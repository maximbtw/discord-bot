using System.Linq;
using Bot.Application.ChatAi.OpenRouter;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Bot.Host;

internal static class LoggingConfigurator
{
    private const string Folder = "logs";

    private static readonly string[] SpecialLoggers =
    {
        nameof(ChatAiOpenRouteLogger),
        "Microsoft.EntityFrameworkCore.Database.Command"
    };

    internal static ILoggerFactory CreateLoggerFactory()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()

            // General: все кроме специальных логеров
            .WriteFiltered("general", console: true, excludeFilters: SpecialLoggers)

            // Специальные логеры
            .WriteFiltered("open-router-requests", includeFilters: new[] { nameof(ChatAiOpenRouteLogger) })
            .WriteFiltered("sql-requests",
                includeFilters: new[] { "Microsoft.EntityFrameworkCore.Database.Command" },
                outputTemplate: "[{Timestamp:HH:mm:ss}] [SQL] {Message:lj}{NewLine}"
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
        bool console = false,
        string[]? includeFilters = null,
        string[]? excludeFilters = null,
        RollingInterval rollingInterval = RollingInterval.Day,
        string? outputTemplate = null)
    {
        return logger.WriteTo.Logger(lc =>
        {
            if (includeFilters is { Length: > 0 })
                lc = lc.Filter.ByIncludingOnly(evt => includeFilters.Any(f => Filter(evt, f)));

            if (excludeFilters is { Length: > 0 })
                lc = lc.Filter.ByExcluding(evt => excludeFilters.Any(f => Filter(evt, f)));

            if (string.IsNullOrEmpty(outputTemplate))
            {
                lc = lc.WriteToFile(fileName, rollingInterval);
            }
            else
            {
                string path = $"{Folder}/{fileName}-.log";
                lc = lc.WriteTo.File(
                    path,
                    rollingInterval: rollingInterval,
                    outputTemplate: outputTemplate
                );
            }

            if (console)
                lc = lc.WriteTo.Console();

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
