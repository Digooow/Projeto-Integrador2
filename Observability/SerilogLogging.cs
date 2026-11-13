using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Projeto_Integrador2.Observability;

public static class SerilogBootstrap
{
    public static Serilog.ILogger CreateLogger() =>
        new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(new JsonFormatter(renderMessage: true))
            .CreateLogger();
}

public sealed class SerilogLoggerProvider(Serilog.ILogger serilogLogger) : ILoggerProvider
{
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) =>
        new SerilogBridgeLogger(serilogLogger.ForContext("SourceContext", categoryName));

    public void Dispose()
    {
        (serilogLogger as IDisposable)?.Dispose();
    }

    private sealed class SerilogBridgeLogger(Serilog.ILogger logger) : Microsoft.Extensions.Logging.ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logger.IsEnabled(MapLevel(logLevel));

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            logger.Write(MapLevel(logLevel), exception, "{Message}", message);
        }

        private static LogEventLevel MapLevel(LogLevel level) => level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}

public static class RequestLoggingExtensions
{
    public const string CorrelationHeader = "X-Correlation-Id";

    public static WebApplication UseStructuredRequestLogging(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString("n");

            context.TraceIdentifier = correlationId;
            context.Response.Headers[CorrelationHeader] = correlationId;

            var sw = Stopwatch.StartNew();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Projeto_Integrador2.HttpRequest");

            try
            {
                await next();
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger.LogError(
                    ex,
                    "HTTP {Method} {Path} failed after {ElapsedMs}ms (CorrelationId={CorrelationId})",
                    context.Request.Method,
                    context.Request.Path.Value,
                    sw.ElapsedMilliseconds,
                    correlationId);
                throw;
            }

            sw.Stop();
            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms (CorrelationId={CorrelationId})",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        });

        return app;
    }
}
