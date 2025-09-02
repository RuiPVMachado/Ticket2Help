using BLL.Configuration;
using BLL.Infrastructure;
using BLL.Services;
using BLL.Validation;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MODELS;
using Moq;

namespace IntegrationTests;

/// <summary>
/// Testes de integração para o sistema completo
/// </summary>
public class TicketSystemIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public TicketSystemIntegrationTests()
    {
        // Configuração para testes
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Application:Name"] = "Ticket2Help",
                ["Application:Environment"] = "Test",
                ["Application:Logging:MinimumLevel"] = "Debug",
                ["Database:ConnectionString"] = "Data Source=.;Initial Catalog=Tick2DeskDB_Test;Integrated Security=True;Encrypt=False;Trust Server Certificate=True"
            });

        _configuration = configBuilder.Build();

        // Setup do container de DI
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configuração
        services.AddSingleton(_configuration);
        BLL.Configuration.ConfigurationManager.RegisterServices(services, _configuration);

        // Logging
        var logger = LoggingConfiguration.ConfigureForTesting();
        services.AddSingleton<Serilog.ILogger>(logger);
        var loggerFactory = LoggingConfiguration.CreateLoggerFactory(logger);
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddLogging();

        // Validação
        services.AddValidation();

        // Repositórios (usando mocks para testes)
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IDetalhesHardwareRepository, DetalhesHardwareRepository>();
        services.AddScoped<IDetalhesSoftwareRepository, DetalhesSoftwareRepository>();
        services.AddScoped<IUtilizadorRepository, UtilizadorRepository>();

        // Serviços
        services.AddScoped<TicketService>();
        services.AddScoped<UtilizadorService>();
    }

    [Fact]
    public void ServiceProvider_ShouldResolveAllServices()
    {
        // Arrange & Act
        var ticketService = _serviceProvider.GetService<TicketService>();
        var utilizadorService = _serviceProvider.GetService<UtilizadorService>();
        var validationService = _serviceProvider.GetService<ValidationService>();
        var configManager = _serviceProvider.GetService<BLL.Configuration.ConfigurationManager>();

        // Assert
        Assert.NotNull(ticketService);
        Assert.NotNull(utilizadorService);
        Assert.NotNull(validationService);
        Assert.NotNull(configManager);
    }

    [Fact]
    public void Configuration_ShouldLoadCorrectly()
    {
        // Arrange & Act
        var configManager = _serviceProvider.GetRequiredService<BLL.Configuration.ConfigurationManager>();

        // Assert
        Assert.Equal("Ticket2Help", configManager.Application.Name);
        Assert.Equal("Test", configManager.Application.Environment);
        Assert.NotEmpty(configManager.Database.ConnectionString);
    }

    [Fact]
    public void Validation_ShouldWorkWithDependencyInjection()
    {
        // Arrange
        var validationService = _serviceProvider.GetRequiredService<ValidationService>();
        var request = new CreateHardwareTicketRequest
        {
            Titulo = "Teste",
            Descricao = "Descrição de teste com mais de 10 caracteres",
            Prioridade = Prioridade.Media,
            ColaboradorId = 1,
            Equipamento = "PC Desktop",
            Avaria = "Não liga",
            CriadoPorId = 1
        };

        // Act
        var result = validationService.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validation_ShouldFailWithInvalidData()
    {
        // Arrange
        var validationService = _serviceProvider.GetRequiredService<ValidationService>();
        var request = new CreateHardwareTicketRequest
        {
            Titulo = "", // Inválido
            Descricao = "Curto", // Muito curto
            ColaboradorId = 0, // Inválido
            Equipamento = "",
            Avaria = "",
            CriadoPorId = 0
        };

        // Act
        var result = validationService.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("Título é obrigatório"));
        Assert.Contains(result.Errors, e => e.Contains("Descrição deve ter entre 10 e 1000 caracteres"));
    }

    [Fact]
    public void Logging_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var logger = _serviceProvider.GetRequiredService<ILogger<TicketSystemIntegrationTests>>();

        // Act & Assert - Não deve lançar exceção
        logger.LogInformation("Teste de logging de integração");
        logger.LogWarning("Teste de warning");
        logger.LogError("Teste de error");

        // Se chegou até aqui, o logging está funcionando
        Assert.True(true);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
