using BLL;
using MODELS;
using Xunit;

namespace Tests
{
    /// <summary>
    /// Testes para a criação de objetos através do TicketFactory
    /// </summary>
    public class TicketFactoryTests
    {
        [Fact]
        public void CriarTicketHardware_DeveRetornarTicketEDetalhesValidos()
        {
            // Arrange
            string titulo = "Problema no Monitor";
            string descricao = "Monitor não liga";
            Prioridade prioridade = Prioridade.Alta;
            int colaboradorId = 1;
            string equipamento = "Monitor Dell 24''";
            string avaria = "Não recebe sinal";
            int criadoPorId = 2;
            bool criadoPorTecnico = true;

            // Act
            var (ticket, detalhes) = TicketFactory.CriarTicketHardware(
                titulo, descricao, prioridade, colaboradorId,
                equipamento, avaria, criadoPorId, criadoPorTecnico);

            // Assert - Verificar Ticket
            Assert.NotNull(ticket);
            Assert.Equal(titulo, ticket.Titulo);
            Assert.Equal(descricao, ticket.Descricao);
            Assert.Equal(prioridade, ticket.Prioridade);
            Assert.Equal(TipoTicket.Hardware, ticket.Tipo);
            Assert.Equal(colaboradorId, ticket.IdColaborador);
            Assert.Equal(criadoPorId, ticket.IdTecnico);
            Assert.Equal(EstadoTicket.EmAtendimento, ticket.Estado); // Criado por técnico
            Assert.NotNull(ticket.DataCriacao);
            Assert.NotNull(ticket.DataAtendimento); // Deve ter data de atendimento se criado por técnico

            // Assert - Verificar Detalhes Hardware
            Assert.NotNull(detalhes);
            Assert.Equal(equipamento, detalhes.Equipamento);
            Assert.Equal(avaria, detalhes.Avaria);
            Assert.Null(detalhes.PecaSubstituida); // Inicialmente null
            Assert.Null(detalhes.DescricaoReparacao); // Inicialmente null
        }

        [Fact]
        public void CriarTicketSoftware_DeveRetornarTicketEDetalhesValidos()
        {
            // Arrange
            string titulo = "Instalação Office";
            string descricao = "Preciso do Microsoft Office";
            Prioridade prioridade = Prioridade.Media;
            int colaboradorId = 3;
            string aplicacao = "Microsoft Office 365";
            string necessidade = "Necessário para trabalhar com documentos";
            int criadoPorId = 3; // Mesmo ID (colaborador cria para si)
            bool criadoPorTecnico = false;

            // Act
            var (ticket, detalhes) = TicketFactory.CriarTicketSoftware(
                titulo, descricao, prioridade, colaboradorId,
                aplicacao, necessidade, criadoPorId, criadoPorTecnico);

            // Assert - Verificar Ticket
            Assert.NotNull(ticket);
            Assert.Equal(titulo, ticket.Titulo);
            Assert.Equal(descricao, ticket.Descricao);
            Assert.Equal(prioridade, ticket.Prioridade);
            Assert.Equal(TipoTicket.Software, ticket.Tipo);
            Assert.Equal(colaboradorId, ticket.IdColaborador);
            Assert.Null(ticket.IdTecnico); // Não criado por técnico
            Assert.Equal(EstadoTicket.PorAtender, ticket.Estado); // Não criado por técnico
            Assert.NotNull(ticket.DataCriacao);
            Assert.Null(ticket.DataAtendimento); // Não deve ter data de atendimento inicialmente

            // Assert - Verificar Detalhes Software
            Assert.NotNull(detalhes);
            Assert.Equal(aplicacao, detalhes.Aplicacao);
            Assert.Equal(necessidade, detalhes.DescricaoNecessidade);
            Assert.Null(detalhes.SolucaoAplicada); // Inicialmente null
        }

        [Theory]
        [InlineData("", "Descrição válida", "Título não pode estar vazio")]
        [InlineData("Título válido", "", "Descrição não pode estar vazia")]
        [InlineData("Título válido", "Descrição válida", null)] // Caso válido
        public void CriarTicket_DeveValidarParametrosObrigatorios(string titulo, string descricao, string? erroEsperado)
        {
            // Arrange
            Prioridade prioridade = Prioridade.Baixa;
            int colaboradorId = 1;
            int criadoPorId = 1;

            if (erroEsperado == null)
            {
                // Act & Assert - Caso válido
                var ticket = TicketFactory.CriarTicket(TipoTicket.Hardware, titulo, descricao, prioridade, colaboradorId, criadoPorId);
                Assert.NotNull(ticket);
                Assert.Equal(titulo, ticket.Titulo);
                Assert.Equal(descricao, ticket.Descricao);
            }
            else
            {
                // Act & Assert - Casos inválidos
                var exception = Assert.Throws<ArgumentException>(() =>
                    TicketFactory.CriarTicket(TipoTicket.Hardware, titulo, descricao, prioridade, colaboradorId, criadoPorId));

                Assert.Contains(erroEsperado, exception.Message);
            }
        }
    }
}