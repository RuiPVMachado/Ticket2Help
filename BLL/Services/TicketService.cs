using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLL.Interfaces;
using DAL.Interfaces;
using MODELS;
using System;
using System.Collections.Generic;

namespace BLL.Services
{
    /// <summary>
    /// Serviço responsável pela lógica de negócio associada aos tickets.
    /// Utiliza o TicketFactory para criação consistente de tickets.
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepo;
        private readonly IDetalhesHardwareRepository _hwRepo;
        private readonly IDetalhesSoftwareRepository _swRepo;

        /// <summary>
        /// Construtor com injeção dos repositórios de tickets e detalhes.
        /// </summary>
        public TicketService(
            ITicketRepository ticketRepo,
            IDetalhesHardwareRepository hwRepo,
            IDetalhesSoftwareRepository swRepo)
        {
            _ticketRepo = ticketRepo;
            _hwRepo = hwRepo;
            _swRepo = swRepo;
        }

        /// <inheritdoc />
        public int CriarTicketHardware(string titulo, string descricao, Prioridade prioridade,
            int colaboradorId, string equipamento, string avaria, int criadoPorId, bool criadoPorTecnico = false)
        {
            try
            {
                // Utiliza o factory para criar o ticket e detalhes
                var (ticket, detalhes) = TicketFactory.CriarTicketHardware(
                    titulo, descricao, prioridade, colaboradorId, equipamento, avaria, criadoPorId, criadoPorTecnico);

                // Persiste o ticket
                int ticketId = _ticketRepo.Criar(ticket);

                // Define o ID do ticket nos detalhes e persiste
                detalhes.TicketId = ticketId;
                _hwRepo.Criar(detalhes);

                return ticketId;
            }
            catch (ArgumentException)
            {
                // Re-lança exceções de validação do factory
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao criar ticket de hardware", ex);
            }
        }

        /// <inheritdoc />
        public int CriarTicketSoftware(string titulo, string descricao, Prioridade prioridade,
            int colaboradorId, string aplicacao, string necessidade, int criadoPorId, bool criadoPorTecnico = false)
        {
            try
            {
                // Utiliza o factory para criar o ticket e detalhes
                var (ticket, detalhes) = TicketFactory.CriarTicketSoftware(
                    titulo, descricao, prioridade, colaboradorId, aplicacao, necessidade, criadoPorId, criadoPorTecnico);

                // Persiste o ticket
                int ticketId = _ticketRepo.Criar(ticket);

                // Define o ID do ticket nos detalhes e persiste
                detalhes.TicketId = ticketId;
                _swRepo.Criar(detalhes);

                return ticketId;
            }
            catch (ArgumentException)
            {
                // Re-lança exceções de validação do factory
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao criar ticket de software", ex);
            }
        }

        /// <summary>
        /// Métodos de compatibilidade - mantêm a assinatura original para não quebrar código existente
        /// </summary>
        public int CriarTicketHardware(string titulo, string descricao, Prioridade prioridade,
            int colaboradorId, string equipamento, string avaria)
        {
            return CriarTicketHardware(titulo, descricao, prioridade, colaboradorId,
                                     equipamento, avaria, colaboradorId, false);
        }

        public int CriarTicketSoftware(string titulo, string descricao, Prioridade prioridade,
            int colaboradorId, string aplicacao, string necessidade)
        {
            return CriarTicketSoftware(titulo, descricao, prioridade, colaboradorId,
                                     aplicacao, necessidade, colaboradorId, false);
        }

        /// <inheritdoc />
        [Obsolete("Use CriarTicketHardware ou CriarTicketSoftware com o factory pattern")]
        public int CriarTicket(Ticket ticket, object detalhes)
        {
            int ticketId = _ticketRepo.Criar(ticket);

            if (ticket.Tipo == TipoTicket.Hardware && detalhes is DetalhesHardware hw)
            {
                hw.TicketId = ticketId;
                _hwRepo.Criar(hw);
            }
            else if (ticket.Tipo == TipoTicket.Software && detalhes is DetalhesSoftware sw)
            {
                sw.TicketId = ticketId;
                _swRepo.Criar(sw);
            }

            return ticketId;
        }

        /// <inheritdoc />
        public bool AtualizarEstado(int ticketId, EstadoTicket novoEstado, SubEstadoAtendimento? subEstado, int tecnicoId, object? detalhes = null)
        {
            try
            {
                bool atualizado = _ticketRepo.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId);

                if (!atualizado || novoEstado != EstadoTicket.Atendido)
                    return atualizado;

                // Atualiza detalhes específicos se fornecidos
                if (detalhes is DetalhesHardware hw)
                {
                    hw.TicketId = ticketId;
                    return _hwRepo.Atualizar(hw);
                }
                else if (detalhes is DetalhesSoftware sw)
                {
                    sw.TicketId = ticketId;
                    return _swRepo.Atualizar(sw);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao atualizar estado do ticket {ticketId}", ex);
            }
        }

        /// <inheritdoc />
        public Ticket ObterPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser válido", nameof(id));

            return _ticketRepo.ObterPorId(id);
        }

        /// <inheritdoc />
        public List<Ticket> ObterTodos()
        {
            return _ticketRepo.ObterTodos();
        }

        /// <inheritdoc />
        public List<Ticket> ObterPorEstado(EstadoTicket estado)
        {
            return _ticketRepo.ObterPorEstado(estado);
        }

        public bool EliminarTicket(int ticketId)
        {
            // Validar se o ticket existe
            var ticket = _ticketRepo.ObterPorId(ticketId);
            if (ticket == null)
                return false;

            if (ticket.Estado == EstadoTicket.Atendido)
                return false;

            return _ticketRepo.Eliminar(ticketId);
        }
    }
}