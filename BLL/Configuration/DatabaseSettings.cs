namespace BLL.Configuration
{
    /// <summary>
    /// Configurações de base de dados
    /// </summary>
    public class DatabaseSettings
    {
        public const string SectionName = "Database";

        /// <summary>
        /// String de conexão principal
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// String de conexão para testes
        /// </summary>
        public string TestConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Timeout para comandos de base de dados (em segundos)
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Número máximo de tentativas de reconexão
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Intervalo entre tentativas de reconexão (em segundos)
        /// </summary>
        public int RetryIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Habilitar pooling de conexões
        /// </summary>
        public bool EnableConnectionPooling { get; set; } = true;

        /// <summary>
        /// Tamanho mínimo do pool de conexões
        /// </summary>
        public int MinPoolSize { get; set; } = 5;

        /// <summary>
        /// Tamanho máximo do pool de conexões
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;
    }
}