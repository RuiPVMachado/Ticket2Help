using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BLL.Configuration
{
    /// <summary>
    /// Gestor centralizado de configurações da aplicação
    /// </summary>
    public class ConfigurationManager
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationSettings _appSettings;
        private readonly DatabaseSettings _dbSettings;

        /// <summary>
        /// Construtor que inicializa o gestor de configurações
        /// </summary>
        /// <param name="configuration">Instância de IConfiguration</param>
        public ConfigurationManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettings = new ApplicationSettings();
            _dbSettings = new DatabaseSettings();

            // Bind das configurações
            _configuration.GetSection(ApplicationSettings.SectionName).Bind(_appSettings);
            _configuration.GetSection(DatabaseSettings.SectionName).Bind(_dbSettings);
        }

        /// <summary>
        /// Configurações da aplicação
        /// </summary>
        public ApplicationSettings Application => _appSettings;

        /// <summary>
        /// Configurações da base de dados
        /// </summary>
        public DatabaseSettings Database => _dbSettings;

        /// <summary>
        /// Cria um ConfigurationManager a partir de um arquivo JSON
        /// </summary>
        /// <param name="configFilePath">Caminho para o arquivo de configuração</param>
        /// <param name="environment">Ambiente atual (opcional)</param>
        /// <returns>Instância configurada do ConfigurationManager</returns>
        public static ConfigurationManager CreateFromFile(string configFilePath = "appsettings.json", string? environment = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFilePath, optional: false, reloadOnChange: true);

            // Adiciona arquivo específico do ambiente se especificado
            if (!string.IsNullOrEmpty(environment))
            {
                var environmentFile = $"appsettings.{environment}.json";
                builder.AddJsonFile(environmentFile, optional: true, reloadOnChange: true);
            }

            // Adiciona variáveis de ambiente (se disponível)
            try
            {
                builder.AddEnvironmentVariables();
            }
            catch
            {
                // Ignora se não estiver disponível
            }

            var configuration = builder.Build();
            return new ConfigurationManager(configuration);
        }

        /// <summary>
        /// Regista os serviços de configuração no container de DI
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        /// <param name="configuration">Configuração</param>
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Regista as configurações como singletons
            services.Configure<ApplicationSettings>(configuration.GetSection(ApplicationSettings.SectionName));
            services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));

            // Regista o ConfigurationManager
            services.AddSingleton<ConfigurationManager>();
        }

        /// <summary>
        /// Valida se todas as configurações obrigatórias estão presentes
        /// </summary>
        /// <returns>Lista de erros de validação (vazia se tudo estiver correto)</returns>
        public List<string> ValidateConfiguration()
        {
            var errors = new List<string>();

            // Validação das configurações da aplicação
            if (string.IsNullOrWhiteSpace(_appSettings.Name))
                errors.Add("Application:Name é obrigatório");

            if (string.IsNullOrWhiteSpace(_appSettings.Version))
                errors.Add("Application:Version é obrigatório");

            // Validação das configurações da base de dados
            if (string.IsNullOrWhiteSpace(_dbSettings.ConnectionString))
                errors.Add("Database:ConnectionString é obrigatório");

            if (_dbSettings.CommandTimeout <= 0)
                errors.Add("Database:CommandTimeout deve ser maior que zero");

            if (_dbSettings.MaxRetryAttempts < 0)
                errors.Add("Database:MaxRetryAttempts não pode ser negativo");

            // Validação das configurações de logging
            if (_appSettings.Logging.RetainedFileCountLimit <= 0)
                errors.Add("Application:Logging:RetainedFileCountLimit deve ser maior que zero");

            // Validação das configurações de performance
            if (_appSettings.Performance.MaxTicketsPerQuery <= 0)
                errors.Add("Application:Performance:MaxTicketsPerQuery deve ser maior que zero");

            return errors;
        }

        /// <summary>
        /// Obtém uma configuração específica por chave
        /// </summary>
        /// <param name="key">Chave da configuração</param>
        /// <returns>Valor da configuração ou null se não encontrada</returns>
        public string? GetValue(string key)
        {
            return _configuration[key];
        }

        /// <summary>
        /// Obtém uma configuração específica por chave com valor padrão
        /// </summary>
        /// <param name="key">Chave da configuração</param>
        /// <param name="defaultValue">Valor padrão</param>
        /// <returns>Valor da configuração ou valor padrão</returns>
        public string GetValue(string key, string defaultValue)
        {
            return _configuration[key] ?? defaultValue;
        }
    }
}