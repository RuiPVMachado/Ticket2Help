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
            bool atualizado = _ticketRepo.AtualizarEstado(ticketId, novoEstado, subEstado, tecnicoId);

            if (!atualizado || novoEstado != EstadoTicket.Atendido)
                return atualizado;

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

        /// <inheritdoc />
        public Ticket ObterPorId(int id)
        {
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
    }
}
