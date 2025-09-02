using Microsoft.Extensions.Logging;

namespace BLL.Configuration
{
    /// <summary>
    /// Configurações principais da aplicação
    /// </summary>
    public class ApplicationSettings
    {
        public const string SectionName = "Application";

        /// <summary>
        /// Nome da aplicação
        /// </summary>
        public string Name { get; set; } = "Ticket2Help";

        /// <summary>
        /// Versão da aplicação
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Ambiente de execução (Development, Staging, Production)
        /// </summary>
        public string Environment { get; set; } = "Development";

        /// <summary>
        /// Configurações de logging
        /// </summary>
        public LoggingSettings Logging { get; set; } = new();

        /// <summary>
        /// Configurações de performance
        /// </summary>
        public PerformanceSettings Performance { get; set; } = new();
    }

    /// <summary>
    /// Configurações específicas de logging
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Nível mínimo de log
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Caminho para os arquivos de log
        /// </summary>
        public string LogFilePath { get; set; } = "logs/ticket2help-.log";

        /// <summary>
        /// Número de dias para manter os arquivos de log
        /// </summary>
        public int RetainedFileCountLimit { get; set; } = 30;

        /// <summary>
        /// Habilitar logging para console
        /// </summary>
        public bool EnableConsoleLogging { get; set; } = true;

        /// <summary>
        /// Habilitar logging para arquivo
        /// </summary>
        public bool EnableFileLogging { get; set; } = true;
    }

    /// <summary>
    /// Configurações de performance e limites
    /// </summary>
    public class PerformanceSettings
    {
        /// <summary>
        /// Timeout padrão para operações de base de dados (em segundos)
        /// </summary>
        public int DatabaseTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Número máximo de tickets retornados em uma consulta
        /// </summary>
        public int MaxTicketsPerQuery { get; set; } = 1000;

        /// <summary>
        /// Intervalo de cache para consultas frequentes (em minutos)
        /// </summary>
        public int CacheIntervalMinutes { get; set; } = 5;
    }
}