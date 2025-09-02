using BLL.Services;
using DAL.Interfaces;
using MODELS;
using Moq;
using Xunit;

namespace Tests
{
    /// <summary>
    /// Testes unitários para a lógica de negócio do TicketService
    /// </summary>
    public class TicketServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IDetalhesHardwareRepository> _mockHwRepo;
        private readonly Mock<IDetalhesSoftwareRepository> _mockSwRepo;
        private readonly TicketService _service;

        public TicketServiceTests()
        {
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockHwRepo = new Mock<IDetalhesHardwareRepository>();
            _mockSwRepo = new Mock<IDetalhesSoftwareRepository>();
            _service = new TicketService(_mockTicketRepo.Object, _mockHwRepo.Object, _mockSwRepo.Object);
        }

        #region ObterPorId Tests

        [Fact]
        public void ObterPorId_ComIdValido_DeveRetornarTicket()
        {
            // Arrange
            int ticketId = 1;
            var ticketEsperado = new Ticket
            {
                Id = ticketId,
                Titulo = "Teste Ticket",
                Descricao = "Descrição do teste",
                Prioridade = Prioridade.Media,
                Tipo = TipoTicket.Hardware,
                Estado = EstadoTicket.PorAtender,
                DataCriacao = DateTime.Now,
                IdColaborador = 1
            };

            _mockTicketRepo.Setup(r => r.ObterPorId(ticketId))
                          .Returns(ticketEsperado);

            // Act
            var resultado = _service.ObterPorId(ticketId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(ticketEsperado.Id, resultado.Id);
            Assert.Equal(ticketEsperado.Titulo, resultado.Titulo);
            Assert.Equal(ticketEsperado.Descricao, resultado.Descricao);
            _mockTicketRepo.Verify(r => r.ObterPorId(ticketId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void ObterPorId_ComIdInvalido_DeveLancarArgumentException(int idInvalido)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.ObterPorId(idInvalido));
            Assert.Contains("ID deve ser válido", exception.Message);
            Assert.Equal("id", exception.ParamName);
            
            // Verificar que o repositório não foi chamado
            _mockTicketRepo.Verify(r => r.ObterPorId(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region EliminarTicket Tests

        [Fact]
        public void EliminarTicket_ComTicketExistenteEEstadoValido_DeveEliminarComSucesso()
        {
            // Arrange
            int ticketId = 1;
            var ticket = new Ticket
            {
                Id = ticketId,
                Titulo = "Ticket para eliminar",
                Estado = EstadoTicket.PorAtender,
                Tipo = TipoTicket.Hardware,
                Prioridade = Prioridade.Baixa,
                Descricao = "Teste",
                DataCriacao = DateTime.Now,
                IdColaborador = 1
            };

            _mockTicketRepo.Setup(r => r.ObterPorId(ticketId))
                          .Returns(ticket);
            _mockTicketRepo.Setup(r => r.Eliminar(ticketId))
                          .Returns(true);

            // Act
            var resultado = _service.EliminarTicket(ticketId);

            // Assert
            Assert.True(resultado);
            _mockTicketRepo.Verify(r => r.ObterPorId(ticketId), Times.Once);
            _mockTicketRepo.Verify(r => r.Eliminar(ticketId), Times.Once);
        }

        [Fact]
        public void EliminarTicket_ComTicketEmAtendimento_DeveEliminarComSucesso()
        {
            // Arrange
            int ticketId = 2;
            var ticket = new Ticket
            {
                Id = ticketId,
                Titulo = "Ticket em atendimento",
                Estado = EstadoTicket.EmAtendimento,
                Tipo = TipoTicket.Software,
                Prioridade = Prioridade.Alta,
                Descricao = "Teste",
                DataCriacao = DateTime.Now,
                IdColaborador = 1
            };

            _mockTicketRepo.Setup(r => r.ObterPorId(ticketId))
                          .Returns(ticket);
            _mockTicketRepo.Setup(r => r.Eliminar(ticketId))
                          .Returns(true);

            // Act
            var resultado = _service.EliminarTicket(ticketId);

            // Assert
            Assert.True(resultado);
            _mockTicketRepo.Verify(r => r.ObterPorId(ticketId), Times.Once);
            _mockTicketRepo.Verify(r => r.Eliminar(ticketId), Times.Once);
        }

        [Fact]
        public void EliminarTicket_ComTicketAtendido_DeveRetornarFalse()
        {
            // Arrange
            int ticketId = 3;
            var ticket = new Ticket
            {
                Id = ticketId,
                Titulo = "Ticket atendido",
                Estado = EstadoTicket.Atendido,
                Tipo = TipoTicket.Hardware,
                Prioridade = Prioridade.Media,
                Descricao = "Teste",
                DataCriacao = DateTime.Now,
                IdColaborador = 1
            };

            _mockTicketRepo.Setup(r => r.ObterPorId(ticketId))
                          .Returns(ticket);

            // Act
            var resultado = _service.EliminarTicket(ticketId);

            // Assert
            Assert.False(resultado);
            _mockTicketRepo.Verify(r => r.ObterPorId(ticketId), Times.Once);
            _mockTicketRepo.Verify(r => r.Eliminar(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void EliminarTicket_ComTicketInexistente_DeveRetornarFalse()
        {
            // Arrange
            int ticketIdInexistente = 999;

            _mockTicketRepo.Setup(r => r.ObterPorId(ticketIdInexistente))
                          .Returns((Ticket?)null);

            // Act
            var resultado = _service.EliminarTicket(ticketIdInexistente);

            // Assert
            Assert.False(resultado);
            _mockTicketRepo.Verify(r => r.ObterPorId(ticketIdInexistente), Times.Once);
            _mockTicketRepo.Verify(r => r.Eliminar(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void EliminarTicket_ComFalhaNoRepositorio_DeveRetornarFalse()
        {
            // Arrange
            int ticketId = 4;
            var ticket = new Ticket
            {
                Id = ticketId,
                Titulo = "Ticket para teste de falha",
                Estado = EstadoTicket.PorAtender,
                Tipo = TipoTicket.Software,
                Prioridade = Prioridade.Baixa,
                Descricao = "Teste",
                DataCriacao = DateTime.Now,
                IdColaborador = 1
            };

            _mockTicketRepo.Setup(r => r.ObterPorId(ticketId))
                          .Returns(ticket);
            _mockTicketRepo.Setup(r => r.Eliminar(ticketId))
                          .Returns(false); // Simula falha na eliminação

            // Act
            var resultado = _service.EliminarTicket(ticketId);

            // Assert
            Assert.False(resultado);
            _mockTicketRepo.Verify(r => r.ObterPorId(ticketId), Times.Once);
            _mockTicketRepo.Verify(r => r.Eliminar(ticketId), Times.Once);
        }

        #endregion

        #region AtualizarEstado Tests

        [Fact]
        public void AtualizarEstado_ComEstadoNaoAtendido_DeveAtualizarApenasTicker()
        {
            // Arrange
            int ticketId = 1;
            EstadoTicket novoEstado = EstadoTicket.EmAtendimento;
            SubEstadoAtendimento? subEstado = null;
            int tecnicoId = 5;

            _mockTicketRepo.Setup(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId))
                          .Returns(true);

            // Act
            var resultado = _service.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId);

            // Assert
            Assert.True(resultado);
            _mockTicketRepo.Verify(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId), Times.Once);
            _mockHwRepo.Verify(r => r.Atualizar(It.IsAny<DetalhesHardware>()), Times.Never);
            _mockSwRepo.Verify(r => r.Atualizar(It.IsAny<DetalhesSoftware>()), Times.Never);
        }

        [Fact]
        public void AtualizarEstado_ComEstadoAtendidoSemDetalhes_DeveAtualizarApenasTicket()
        {
            // Arrange
            int ticketId = 2;
            EstadoTicket novoEstado = EstadoTicket.Atendido;
            SubEstadoAtendimento? subEstado = SubEstadoAtendimento.Resolvido;
            int tecnicoId = 3;

            _mockTicketRepo.Setup(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId))
                          .Returns(true);

            // Act
            var resultado = _service.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId);

            // Assert
            Assert.True(resultado);
            _mockTicketRepo.Verify(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId), Times.Once);
            _mockHwRepo.Verify(r => r.Atualizar(It.IsAny<DetalhesHardware>()), Times.Never);
            _mockSwRepo.Verify(r => r.Atualizar(It.IsAny<DetalhesSoftware>()), Times.Never);
        }

        [Fact]
        public void AtualizarEstado_ComEstadoAtendidoEDetalhesHardware_DeveAtualizarTicketEDetalhes()
        {
            // Arrange
            int ticketId = 3;
            EstadoTicket novoEstado = EstadoTicket.Atendido;
            SubEstadoAtendimento? subEstado = SubEstadoAtendimento.Resolvido;
            int tecnicoId = 2;
            var detalhesHardware = new DetalhesHardware
            {
                Equipamento = "Monitor Dell",
                Avaria = "Não liga",
                PecaSubstituida = "Cabo de alimentação",
                DescricaoReparacao = "Substituído cabo defeituoso"
            };

            _mockTicketRepo.Setup(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId))
                          .Returns(true);
            _mockHwRepo.Setup(r => r.Atualizar(It.IsAny<DetalhesHardware>()))
                       .Returns(true);

            // Act
            var resultado = _service.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId, detalhesHardware);

            // Assert
            Assert.True(resultado);
            _mockTicketRepo.Verify(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId), Times.Once);
            _mockHwRepo.Verify(r => r.Atualizar(It.Is<DetalhesHardware>(d => 
                d.TicketId == ticketId && 
                d.PecaSubstituida == "Cabo de alimentação" &&
                d.DescricaoReparacao == "Substituído cabo defeituoso")), Times.Once);
            _mockSwRepo.Verify(r => r.Atualizar(It.IsAny<DetalhesSoftware>()), Times.Never);
        }

        [Fact]
        public void AtualizarEstado_ComEstadoAtendidoEDetalhesSoftware_DeveAtualizarTicketEDetalhes()
        {
            // Arrange
            int ticketId = 4;
            EstadoTicket novoEstado = EstadoTicket.Atendido;
            SubEstadoAtendimento? subEstado = SubEstadoAtendimento.Resolvido;
            int tecnicoId = 1;
            var detalhesSoftware = new DetalhesSoftware
            {
                Aplicacao = "Microsoft Office",
                DescricaoNecessidade = "Instalação para trabalho",
                SolucaoAplicada = "Instalado Office 365 com licença corporativa"
            };

            _mockTicketRepo.Setup(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId))
                          .Returns(true);
            _mockSwRepo.Setup(r => r.Atualizar(It.IsAny<DetalhesSoftware>()))
                       .Returns(true);

            // Act
            var resultado = _service.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId, detalhesSoftware);

            // Assert
            Assert.True(resultado);
            _mockTicketRepo.Verify(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId), Times.Once);
            _mockSwRepo.Verify(r => r.Atualizar(It.Is<DetalhesSoftware>(d => 
                d.TicketId == ticketId && 
                d.SolucaoAplicada == "Instalado Office 365 com licença corporativa")), Times.Once);
            _mockHwRepo.Verify(r => r.Atualizar(It.IsAny<DetalhesHardware>()), Times.Never);
        }

        [Fact]
        public void AtualizarEstado_ComFalhaNoRepositorioTicket_DeveRetornarFalse()
        {
            // Arrange
            int ticketId = 5;
            EstadoTicket novoEstado = EstadoTicket.EmAtendimento;
            SubEstadoAtendimento? subEstado = null;
            int tecnicoId = 4;

            _mockTicketRepo.Setup(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId))
                          .Returns(false);

            // Act
            var resultado = _service.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId);

            // Assert
            Assert.False(resultado);
            _mockTicketRepo.Verify(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId), Times.Once);
        }

        [Fact]
        public void AtualizarEstado_ComFalhaNoRepositorioDetalhesHardware_DeveRetornarFalse()
        {
            // Arrange
            int ticketId = 6;
            EstadoTicket novoEstado = EstadoTicket.Atendido;
            SubEstadoAtendimento? subEstado = SubEstadoAtendimento.NaoResolvido;
            int tecnicoId = 3;
            var detalhesHardware = new DetalhesHardware
            {
                Equipamento = "Impressora HP",
                Avaria = "Papel encravado",
                DescricaoReparacao = "Tentativa de reparação falhada"
            };

            _mockTicketRepo.Setup(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId))
                          .Returns(true);
            _mockHwRepo.Setup(r => r.Atualizar(It.IsAny<DetalhesHardware>()))
                       .Returns(false);

            // Act
            var resultado = _service.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId, detalhesHardware);

            // Assert
            Assert.False(resultado);
            _mockTicketRepo.Verify(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId), Times.Once);
            _mockHwRepo.Verify(r => r.Atualizar(It.IsAny<DetalhesHardware>()), Times.Once);
        }

        [Fact]
        public void AtualizarEstado_ComExcecaoNoRepositorio_DeveLancarInvalidOperationException()
        {
            // Arrange
            int ticketId = 7;
            EstadoTicket novoEstado = EstadoTicket.EmAtendimento;
            SubEstadoAtendimento? subEstado = null;
            int tecnicoId = 2;

            _mockTicketRepo.Setup(r => r.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId))
                          .Throws(new Exception("Erro de base de dados"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _service.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId));
            
            Assert.Contains($"Erro ao atualizar estado do ticket {ticketId}", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Erro de base de dados", exception.InnerException.Message);
        }

        #endregion

        #region CriarTicketHardware Error Handling Tests

        [Fact]
        public void CriarTicketHardware_ComExcecaoNoRepositorio_DeveLancarInvalidOperationException()
        {
            // Arrange
            _mockTicketRepo.Setup(r => r.Criar(It.IsAny<Ticket>()))
                          .Throws(new Exception("Erro na base de dados"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _service.CriarTicketHardware("Título", "Descrição", Prioridade.Media, 1, "Equipamento", "Avaria", 1, false));

            Assert.Contains("Erro ao criar ticket de hardware", exception.Message);
            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void CriarTicketHardware_ComParametrosInvalidos_DeveLancarArgumentException()
        {
            // Act & Assert - Título vazio
            var exception = Assert.Throws<ArgumentException>(() =>
                _service.CriarTicketHardware("", "Descrição válida", Prioridade.Media, 1, "Equipamento", "Avaria", 1, false));

            Assert.Contains("Título não pode estar vazio", exception.Message);
        }

        #endregion

        #region CriarTicketSoftware Error Handling Tests

        [Fact]
        public void CriarTicketSoftware_ComExcecaoNoRepositorio_DeveLancarInvalidOperationException()
        {
            // Arrange
            _mockTicketRepo.Setup(r => r.Criar(It.IsAny<Ticket>()))
                          .Throws(new Exception("Erro na base de dados"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _service.CriarTicketSoftware("Título", "Descrição", Prioridade.Alta, 1, "Aplicação", "Necessidade", 1, false));

            Assert.Contains("Erro ao criar ticket de software", exception.Message);
            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void CriarTicketSoftware_ComParametrosInvalidos_DeveLancarArgumentException()
        {
            // Act & Assert - Descrição vazia
            var exception = Assert.Throws<ArgumentException>(() =>
                _service.CriarTicketSoftware("Título válido", "", Prioridade.Baixa, 1, "Aplicação", "Necessidade", 1, false));

            Assert.Contains("Descrição não pode estar vazia", exception.Message);
        }

        #endregion
    }
}