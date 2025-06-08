using BLL.Services;
using DAL.Interfaces;
using MODELS;
using Moq;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    /// <summary>
    /// Testes complexos de performance para o TicketService
    /// </summary>
    public class TicketServicePerformanceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IDetalhesHardwareRepository> _mockHwRepo;
        private readonly Mock<IDetalhesSoftwareRepository> _mockSwRepo;
        private readonly TicketService _service;
        private readonly ITestOutputHelper _output;

        public TicketServicePerformanceTests(ITestOutputHelper output)
        {
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockHwRepo = new Mock<IDetalhesHardwareRepository>();
            _mockSwRepo = new Mock<IDetalhesSoftwareRepository>();
            _service = new TicketService(_mockTicketRepo.Object, _mockHwRepo.Object, _mockSwRepo.Object);
            _output = output;
        }

        [Fact]
        public void CriarMultiplosTicketsHardware_DeveExecutarEmTempoAceitavel()
        {
            // Arrange
            const int numeroTickets = 1000;
            const int tempoMaximoMs = 5000; // 5 segundos máximo

            _mockTicketRepo.Setup(r => r.Criar(It.IsAny<Ticket>()))
                          .Returns((Ticket t) => Random.Shared.Next(1, 10000));

            _mockHwRepo.Setup(r => r.Criar(It.IsAny<DetalhesHardware>()))
                       .Returns((DetalhesHardware d) => Random.Shared.Next(1, 10000));

            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < numeroTickets; i++)
            {
                _service.CriarTicketHardware(
                    $"Ticket Hardware {i}",
                    $"Descrição do problema {i}",
                    (Prioridade)(i % 3), // Alterna entre prioridades
                    i + 1, // ColaboradorId
                    $"Equipamento {i}",
                    $"Avaria {i}",
                    (i % 2 == 0) ? i + 1 : i + 2, // CriadoPorId
                    i % 4 == 0 // 25% criados por técnico
                );
            }

            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Tempo para criar {numeroTickets} tickets: {stopwatch.ElapsedMilliseconds}ms");
            Assert.True(stopwatch.ElapsedMilliseconds < tempoMaximoMs,
                       $"Criação de {numeroTickets} tickets demorou {stopwatch.ElapsedMilliseconds}ms, " +
                       $"excedendo o limite de {tempoMaximoMs}ms");

            // Verificar que os métodos foram chamados o número correto de vezes
            _mockTicketRepo.Verify(r => r.Criar(It.IsAny<Ticket>()), Times.Exactly(numeroTickets));
            _mockHwRepo.Verify(r => r.Criar(It.IsAny<DetalhesHardware>()), Times.Exactly(numeroTickets));
        }

        [Fact]
        public void ObterTicketsPorEstado_ComGrandeVolumeDeDados_DeveSerEficiente()
        {
            // Arrange
            const int numeroTickets = 10000;
            const int tempoMaximoMs = 1000; // 1 segundo máximo

            var ticketsSimulados = GerarTicketsParaTeste(numeroTickets);

            _mockTicketRepo.Setup(r => r.ObterPorEstado(EstadoTicket.PorAtender))
                          .Returns(ticketsSimulados.Where(t => t.Estado == EstadoTicket.PorAtender).ToList());

            var stopwatch = Stopwatch.StartNew();

            // Act
            var ticketsPorAtender = _service.ObterPorEstado(EstadoTicket.PorAtender);

            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Tempo para filtrar {numeroTickets} tickets: {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Tickets encontrados: {ticketsPorAtender.Count}");

            Assert.True(stopwatch.ElapsedMilliseconds < tempoMaximoMs,
                       $"Filtrar tickets demorou {stopwatch.ElapsedMilliseconds}ms, " +
                       $"excedendo o limite de {tempoMaximoMs}ms");

            Assert.True(ticketsPorAtender.Count > 0, "Deveria retornar pelo menos alguns tickets");
            Assert.True(ticketsPorAtender.All(t => t.Estado == EstadoTicket.PorAtender),
                       "Todos os tickets retornados devem ter estado 'PorAtender'");
        }

        [Fact]
        public void AtualizarEstadoMultiplosTickets_DeveProcessarEmLote()
        {
            // Arrange
            const int numeroTickets = 500;
            const int tempoMaximoMs = 3000; // 3 segundos máximo

            var idsTickets = Enumerable.Range(1, numeroTickets).ToList();

            _mockTicketRepo.Setup(r => r.AtualizarEstado(
                It.IsAny<int>(),
                It.IsAny<EstadoTicket>(),
                It.IsAny<SubEstadoAtendimento?>(),
                It.IsAny<int?>()))
                .Returns(true);

            var stopwatch = Stopwatch.StartNew();

            // Act
            int sucessos = 0;
            foreach (var ticketId in idsTickets)
            {
                bool sucesso = _service.AtualizarEstado(
                    ticketId,
                    EstadoTicket.EmAtendimento,
                    null,
                    1); // Técnico ID 1

                if (sucesso) sucessos++;
            }

            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Tempo para atualizar {numeroTickets} tickets: {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Taxa de sucesso: {sucessos}/{numeroTickets} ({(double)sucessos / numeroTickets * 100:F1}%)");

            Assert.True(stopwatch.ElapsedMilliseconds < tempoMaximoMs,
                       $"Atualização de {numeroTickets} tickets demorou {stopwatch.ElapsedMilliseconds}ms, " +
                       $"excedendo o limite de {tempoMaximoMs}ms");

            Assert.Equal(numeroTickets, sucessos);

            _mockTicketRepo.Verify(r => r.AtualizarEstado(
                It.IsAny<int>(),
                EstadoTicket.EmAtendimento,
                null,
                1), Times.Exactly(numeroTickets));
        }

        [Theory]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void CriarTicketsSoftware_ComDiferentesVolumes_DeveEscalarLinearmente(int numeroTickets)
        {
            // Arrange
            _mockTicketRepo.Setup(r => r.Criar(It.IsAny<Ticket>()))
                          .Returns((Ticket t) => Random.Shared.Next(1, 10000));

            _mockSwRepo.Setup(r => r.Criar(It.IsAny<DetalhesSoftware>()))
                       .Returns((DetalhesSoftware d) => Random.Shared.Next(1, 10000));

            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < numeroTickets; i++)
            {
                _service.CriarTicketSoftware(
                    $"Ticket Software {i}",
                    $"Necessidade {i}",
                    (Prioridade)(i % 3),
                    i + 1,
                    $"Aplicação {i}",
                    $"Descrição necessidade {i}"
                );
            }

            stopwatch.Stop();

            // Assert
            double tempoMedioPorTicket = (double)stopwatch.ElapsedMilliseconds / numeroTickets;

            _output.WriteLine($"Volume: {numeroTickets} tickets");
            _output.WriteLine($"Tempo total: {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Tempo médio por ticket: {tempoMedioPorTicket:F2}ms");

            // Verificar que o tempo médio por ticket não excede 10ms
            Assert.True(tempoMedioPorTicket < 10.0,
                       $"Tempo médio por ticket ({tempoMedioPorTicket:F2}ms) excede o limite de 10ms");
        }

        /// <summary>
        /// Método auxiliar para gerar tickets de teste
        /// </summary>
        private List<Ticket> GerarTicketsParaTeste(int quantidade)
        {
            var tickets = new List<Ticket>();
            var random = new Random(42); // Seed fixo para resultados consistentes

            for (int i = 0; i < quantidade; i++)
            {
                tickets.Add(new Ticket
                {
                    Id = i + 1,
                    Titulo = $"Ticket {i}",
                    Descricao = $"Descrição do ticket {i}",
                    Prioridade = (Prioridade)(i % 3),
                    Tipo = (TipoTicket)(i % 2),
                    Estado = (EstadoTicket)(i % 3),
                    DataCriacao = DateTime.Now.AddDays(-random.Next(0, 30)),
                    IdColaborador = random.Next(1, 100),
                    IdTecnico = i % 4 == 0 ? random.Next(1, 10) : null
                });
            }

            return tickets;
        }
    }
}