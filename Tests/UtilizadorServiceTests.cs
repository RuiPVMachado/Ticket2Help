using BLL.Services;
using DAL.Interfaces;
using MODELS;
using Moq;
using Xunit;

namespace Tests
{
    /// <summary>
    /// Testes para validação da função ValidarLogin do UtilizadorService
    /// </summary>
    public class UtilizadorServiceTests
    {
        private readonly Mock<IUtilizadorRepository> _mockRepo;
        private readonly UtilizadorService _service;

        public UtilizadorServiceTests()
        {
            _mockRepo = new Mock<IUtilizadorRepository>();
            _service = new UtilizadorService(_mockRepo.Object);
        }

        [Fact]
        public void ValidarLogin_ComCredenciaisValidas_DeveRetornarUtilizador()
        {
            // Arrange
            string username = "joao.silva";
            string password = "senha123";

            var utilizadorEsperado = new Utilizador
            {
                Id = 1,
                Nome = "João Silva",
                Username = username,
                PasswordHash = password,
                Tipo = TipoUtilizador.Tecnico,
                DataCriacao = DateTime.Now
            };

            _mockRepo.Setup(r => r.ObterPorUsername(username))
                     .Returns(utilizadorEsperado);

            // Act
            var resultado = _service.ValidarLogin(username, password);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(utilizadorEsperado.Id, resultado.Id);
            Assert.Equal(utilizadorEsperado.Nome, resultado.Nome);
            Assert.Equal(utilizadorEsperado.Username, resultado.Username);
            Assert.Equal(utilizadorEsperado.Tipo, resultado.Tipo);

            // Verificar que o repositório foi chamado corretamente
            _mockRepo.Verify(r => r.ObterPorUsername(username), Times.Once);
        }

        [Fact]
        public void ValidarLogin_ComPasswordIncorreta_DeveRetornarNull()
        {
            // Arrange
            string username = "joao.silva";
            string passwordCorreta = "senha123";
            string passwordIncorreta = "senhaErrada";

            var utilizador = new Utilizador
            {
                Id = 1,
                Nome = "João Silva",
                Username = username,
                PasswordHash = passwordCorreta,
                Tipo = TipoUtilizador.Tecnico,
                DataCriacao = DateTime.Now
            };

            _mockRepo.Setup(r => r.ObterPorUsername(username))
                     .Returns(utilizador);

            // Act
            var resultado = _service.ValidarLogin(username, passwordIncorreta);

            // Assert
            Assert.Null(resultado);
            _mockRepo.Verify(r => r.ObterPorUsername(username), Times.Once);
        }

        [Fact]
        public void ValidarLogin_ComUsernameInexistente_DeveRetornarNull()
        {
            // Arrange
            string usernameInexistente = "utilizador.inexistente";
            string password = "qualquerSenha";

            _mockRepo.Setup(r => r.ObterPorUsername(usernameInexistente))
                     .Returns((Utilizador?)null);

            // Act
            var resultado = _service.ValidarLogin(usernameInexistente, password);

            // Assert
            Assert.Null(resultado);
            _mockRepo.Verify(r => r.ObterPorUsername(usernameInexistente), Times.Once);
        }

        [Theory]
        [InlineData("admin", "admin123", TipoUtilizador.Tecnico)]
        [InlineData("user.colaborador", "user456", TipoUtilizador.Colaborador)]
        public void ValidarLogin_ComDiferentesTiposUtilizador_DeveRetornarCorretamente(
            string username, string password, TipoUtilizador tipo)
        {
            // Arrange
            var utilizador = new Utilizador
            {
                Id = 1,
                Nome = "Utilizador Teste",
                Username = username,
                PasswordHash = password,
                Tipo = tipo,
                DataCriacao = DateTime.Now
            };

            _mockRepo.Setup(r => r.ObterPorUsername(username))
                     .Returns(utilizador);

            // Act
            var resultado = _service.ValidarLogin(username, password);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tipo, resultado.Tipo);
        }

        [Fact]
        public void ObterTodos_DeveRetornarListaDeUtilizadores()
        {
            // Arrange
            var utilizadoresEsperados = new List<Utilizador>
            {
                new() { Id = 1, Nome = "João Silva", Username = "joao.silva", Tipo = TipoUtilizador.Tecnico },
                new() { Id = 2, Nome = "Maria Santos", Username = "maria.santos", Tipo = TipoUtilizador.Colaborador }
            };

            _mockRepo.Setup(r => r.ObterTodos())
                     .Returns(utilizadoresEsperados);

            // Act
            var resultado = _service.ObterTodos();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            Assert.Equal(utilizadoresEsperados[0].Nome, resultado[0].Nome);
            Assert.Equal(utilizadoresEsperados[1].Nome, resultado[1].Nome);

            _mockRepo.Verify(r => r.ObterTodos(), Times.Once);
        }
    }
}