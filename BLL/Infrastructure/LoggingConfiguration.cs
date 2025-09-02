using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace BLL.Infrastructure
{
    /// <summary>
    /// Configuração centralizada do sistema de logging usando Serilog
    /// </summary>
    public static class LoggingConfiguration
    {
        /// <summary>
        /// Configura o Serilog com as configurações padrão do sistema
        /// </summary>
        /// <param name="logLevel">Nível mínimo de log (padrão: Information)</param>
        /// <param name="logFilePath">Caminho para o arquivo de log (padrão: logs/ticket2help-.log)</param>
        /// <returns>Logger configurado</returns>
        public static Serilog.ILogger ConfigureSerilog(
            LogLevel logLevel = LogLevel.Information,
            string logFilePath = "logs/ticket2help-.log")
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(ConvertLogLevel(logLevel))
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Ticket2Help")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}");

            return loggerConfig.CreateLogger();
        }

        /// <summary>
        /// Cria um ILoggerFactory configurado com Serilog
        /// </summary>
        /// <param name="serilogLogger">Logger Serilog configurado</param>
        /// <returns>ILoggerFactory para injeção de dependência</returns>
        public static ILoggerFactory CreateLoggerFactory(Serilog.ILogger serilogLogger)
        {
            return new SerilogLoggerFactory(serilogLogger);
        }

        /// <summary>
        /// Converte LogLevel do Microsoft.Extensions.Logging para Serilog.Events.LogEventLevel
        /// </summary>
        private static Serilog.Events.LogEventLevel ConvertLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
                LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
                LogLevel.Information => Serilog.Events.LogEventLevel.Information,
                LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
                LogLevel.Error => Serilog.Events.LogEventLevel.Error,
                LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
                LogLevel.None => Serilog.Events.LogEventLevel.Fatal,
                _ => Serilog.Events.LogEventLevel.Information
            };
        }

        /// <summary>
        /// Configuração específica para testes - apenas console, sem arquivos
        /// </summary>
        /// <returns>Logger configurado para testes</returns>
        public static Serilog.ILogger ConfigureForTesting()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}