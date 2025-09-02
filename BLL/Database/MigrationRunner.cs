using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;

namespace BLL.Database
{
    /// <summary>
    /// Executor de migrações de base de dados
    /// </summary>
    public class MigrationRunner
    {
        private readonly string _connectionString;
        private readonly ILogger<MigrationRunner> _logger;
        private const string MigrationsTableName = "__MigrationHistory";

        public MigrationRunner(string connectionString, ILogger<MigrationRunner>? logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<MigrationRunner>.Instance;
        }

        /// <summary>
        /// Executa todas as migrações pendentes
        /// </summary>
        /// <returns>Lista de resultados das migrações executadas</returns>
        public async Task<List<MigrationResult>> RunPendingMigrationsAsync()
        {
            _logger.LogInformation("Iniciando execução de migrações pendentes");

            var results = new List<MigrationResult>();

            try
            {
                await EnsureMigrationTableExistsAsync();
                
                var allMigrations = GetAllMigrations();
                var appliedMigrations = await GetAppliedMigrationsAsync();
                var pendingMigrations = GetPendingMigrations(allMigrations, appliedMigrations);

                _logger.LogInformation("Encontradas {Count} migrações pendentes", pendingMigrations.Count);

                foreach (var migration in pendingMigrations.OrderBy(m => m.Version))
                {
                    var result = await ExecuteMigrationAsync(migration, isUp: true);
                    results.Add(result);

                    if (!result.Success)
                    {
                        _logger.LogError("Migração {Version} falhou. Parando execução.", migration.Version);
                        break;
                    }
                }

                _logger.LogInformation("Execução de migrações concluída. {SuccessCount}/{TotalCount} bem-sucedidas", 
                    results.Count(r => r.Success), results.Count);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante execução de migrações");
                throw;
            }
        }

        /// <summary>
        /// Reverte uma migração específica
        /// </summary>
        /// <param name="version">Versão da migração a reverter</param>
        /// <returns>Resultado da operação</returns>
        public async Task<MigrationResult> RollbackMigrationAsync(string version)
        {
            _logger.LogInformation("Iniciando rollback da migração {Version}", version);

            try
            {
                var migration = GetMigrationByVersion(version);
                if (migration == null)
                {
                    return MigrationResult.CreateFailure(version, "", $"Migração {version} não encontrada");
                }

                if (!migration.CanRollback)
                {
                    return MigrationResult.CreateFailure(version, migration.Description, 
                        "Esta migração não pode ser revertida");
                }

                var appliedMigrations = await GetAppliedMigrationsAsync();
                if (!appliedMigrations.Any(m => m.Version == version))
                {
                    return MigrationResult.CreateFailure(version, migration.Description, 
                        "Migração não foi aplicada anteriormente");
                }

                return await ExecuteMigrationAsync(migration, isUp: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante rollback da migração {Version}", version);
                return MigrationResult.CreateFailure(version, "", ex.Message);
            }
        }

        /// <summary>
        /// Obtém o status de todas as migrações
        /// </summary>
        /// <returns>Lista com status das migrações</returns>
        public async Task<List<MigrationStatus>> GetMigrationStatusAsync()
        {
            var allMigrations = GetAllMigrations();
            var appliedMigrations = await GetAppliedMigrationsAsync();

            var status = new List<MigrationStatus>();

            foreach (var migration in allMigrations.OrderBy(m => m.Version))
            {
                var applied = appliedMigrations.FirstOrDefault(m => m.Version == migration.Version);
                status.Add(new MigrationStatus
                {
                    Version = migration.Version,
                    Description = migration.Description,
                    IsApplied = applied != null,
                    AppliedAt = applied?.AppliedAt,
                    CanRollback = migration.CanRollback,
                    Success = applied?.Success ?? false
                });
            }

            return status;
        }

        private async Task<MigrationResult> ExecuteMigrationAsync(Migration migration, bool isUp)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var operation = isUp ? "aplicar" : "reverter";
            
            _logger.LogInformation("Iniciando {Operation} migração {Version}: {Description}", 
                operation, migration.Version, migration.Description);

            try
            {
                // Validações
                var validationErrors = isUp 
                    ? migration.ValidateBeforeUp(_connectionString)
                    : migration.ValidateBeforeDown(_connectionString);

                if (validationErrors.Any())
                {
                    var errorMessage = string.Join("; ", validationErrors);
                    return MigrationResult.CreateFailure(migration.Version, migration.Description, errorMessage);
                }

                // Execução do script
                var script = isUp ? migration.UpScript : migration.DownScript;
                await ExecuteScriptAsync(script);

                // Atualização da tabela de histórico
                if (isUp)
                {
                    await RecordMigrationAsync(migration, success: true);
                }
                else
                {
                    await RemoveMigrationRecordAsync(migration.Version);
                }

                stopwatch.Stop();
                _logger.LogInformation("Migração {Version} {Operation} com sucesso em {ElapsedMs}ms", 
                    migration.Version, operation == "aplicar" ? "aplicada" : "revertida", stopwatch.ElapsedMilliseconds);

                return MigrationResult.CreateSuccess(migration.Version, migration.Description, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erro ao {Operation} migração {Version}", operation, migration.Version);

                if (isUp)
                {
                    await RecordMigrationAsync(migration, success: false, errorMessage: ex.Message);
                }

                return MigrationResult.CreateFailure(migration.Version, migration.Description, ex.Message);
            }
        }

        private async Task ExecuteScriptAsync(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                return;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Divide o script em comandos individuais (separados por GO)
            var commands = script.Split(new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n" }, 
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var commandText in commands)
            {
                if (string.IsNullOrWhiteSpace(commandText))
                    continue;

                using var command = new SqlCommand(commandText.Trim(), connection);
                command.CommandTimeout = 300; // 5 minutos
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task EnsureMigrationTableExistsAsync()
        {
            var createTableScript = $@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{MigrationsTableName}' AND xtype='U')
                CREATE TABLE [{MigrationsTableName}] (
                    [Version] NVARCHAR(50) NOT NULL PRIMARY KEY,
                    [Description] NVARCHAR(500) NOT NULL,
                    [AppliedAt] DATETIME2 NOT NULL,
                    [AppliedBy] NVARCHAR(100) NOT NULL,
                    [Success] BIT NOT NULL,
                    [ErrorMessage] NVARCHAR(MAX) NULL
                )";

            await ExecuteScriptAsync(createTableScript);
        }

        private async Task<List<AppliedMigration>> GetAppliedMigrationsAsync()
        {
            var migrations = new List<AppliedMigration>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = $"SELECT Version, Description, AppliedAt, AppliedBy, Success, ErrorMessage FROM [{MigrationsTableName}]";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                migrations.Add(new AppliedMigration
                {
                    Version = reader.GetString(0), // Version
                    Description = reader.GetString(1), // Description
                    AppliedAt = reader.GetDateTime(2), // AppliedAt
                    AppliedBy = reader.GetString(3), // AppliedBy
                    Success = reader.GetBoolean(4), // Success
                    ErrorMessage = reader.IsDBNull(5) ? null : reader.GetString(5) // ErrorMessage
                });
            }

            return migrations;
        }

        private async Task RecordMigrationAsync(Migration migration, bool success, string? errorMessage = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
                INSERT INTO [{MigrationsTableName}] (Version, Description, AppliedAt, AppliedBy, Success, ErrorMessage)
                VALUES (@Version, @Description, @AppliedAt, @AppliedBy, @Success, @ErrorMessage)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Version", migration.Version);
            command.Parameters.AddWithValue("@Description", migration.Description);
            command.Parameters.AddWithValue("@AppliedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@AppliedBy", Environment.UserName);
            command.Parameters.AddWithValue("@Success", success);
            command.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        private async Task RemoveMigrationRecordAsync(string version)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = $"DELETE FROM [{MigrationsTableName}] WHERE Version = @Version";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Version", version);

            await command.ExecuteNonQueryAsync();
        }

        private List<Migration> GetAllMigrations()
        {
            var migrations = new List<Migration>();
            var assembly = Assembly.GetExecutingAssembly();

            var migrationTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Migration)) && !t.IsAbstract)
                .ToList();

            foreach (var type in migrationTypes)
            {
                if (Activator.CreateInstance(type) is Migration migration)
                {
                    migrations.Add(migration);
                }
            }

            return migrations;
        }

        private List<Migration> GetPendingMigrations(List<Migration> allMigrations, List<AppliedMigration> appliedMigrations)
        {
            var appliedVersions = appliedMigrations.Where(m => m.Success).Select(m => m.Version).ToHashSet();
            return allMigrations.Where(m => !appliedVersions.Contains(m.Version)).ToList();
        }

        private Migration? GetMigrationByVersion(string version)
        {
            return GetAllMigrations().FirstOrDefault(m => m.Version == version);
        }
    }

    /// <summary>
    /// Status de uma migração
    /// </summary>
    public class MigrationStatus
    {
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsApplied { get; set; }
        public DateTime? AppliedAt { get; set; }
        public bool CanRollback { get; set; }
        public bool Success { get; set; }
    }
}