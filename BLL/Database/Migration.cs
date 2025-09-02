namespace BLL.Database
{
    /// <summary>
    /// Representa uma migração de base de dados
    /// </summary>
    public abstract class Migration
    {
        /// <summary>
        /// Versão da migração (formato: YYYYMMDDHHMMSS)
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// Descrição da migração
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Script SQL para aplicar a migração
        /// </summary>
        public abstract string UpScript { get; }

        /// <summary>
        /// Script SQL para reverter a migração
        /// </summary>
        public abstract string DownScript { get; }

        /// <summary>
        /// Indica se a migração pode ser revertida
        /// </summary>
        public virtual bool CanRollback => !string.IsNullOrEmpty(DownScript);

        /// <summary>
        /// Validações personalizadas antes de aplicar a migração
        /// </summary>
        /// <param name="connectionString">String de conexão</param>
        /// <returns>Lista de erros de validação (vazia se válida)</returns>
        public virtual List<string> ValidateBeforeUp(string connectionString)
        {
            return new List<string>();
        }

        /// <summary>
        /// Validações personalizadas antes de reverter a migração
        /// </summary>
        /// <param name="connectionString">String de conexão</param>
        /// <returns>Lista de erros de validação (vazia se válida)</returns>
        public virtual List<string> ValidateBeforeDown(string connectionString)
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Informações sobre uma migração aplicada
    /// </summary>
    public class AppliedMigration
    {
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public string AppliedBy { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Resultado da execução de uma migração
    /// </summary>
    public class MigrationResult
    {
        public bool Success { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Messages { get; set; } = new();

        public static MigrationResult CreateSuccess(string version, string description, TimeSpan executionTime)
        {
            return new MigrationResult
            {
                Success = true,
                Version = version,
                Description = description,
                ExecutionTime = executionTime
            };
        }

        public static MigrationResult CreateFailure(string version, string description, string errorMessage)
        {
            return new MigrationResult
            {
                Success = false,
                Version = version,
                Description = description,
                ErrorMessage = errorMessage
            };
        }
    }
}