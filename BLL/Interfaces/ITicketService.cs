using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MODELS;
using System.Collections.Generic;

namespace BLL.Interfaces
{
    /// <summary>
    /// Define as operações de negócio relacionadas com tickets.
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Cria um novo ticket (hardware ou software).
        /// </summary>
        /// <param name="ticket">Objeto base do ticket.</param>
        /// <param name="detalhes">Detalhes específicos (hardware/software).</param>
        /// <returns>Id do ticket criado.</returns>
        int CriarTicket(Ticket ticket, object detalhes);

        /// <summary>
        /// Altera o estado de um ticket e regista os detalhes finais.
        /// </summary>
        /// <param name="ticketId">ID do ticket.</param>
        /// <param name="novoEstado">Novo estado (EmAtendimento ou Atendido).</param>
        /// <param name="subEstado">Subestado (Resolvido/NaoResolvido).</param>
        /// <param name="tecnicoId">Id do técnico que atende.</param>
        /// <param name="detalhes">Detalhes HW/SW caso o ticket seja encerrado.</param>
        /// <returns>True se for atualizado com sucesso.</returns>
        bool AtualizarEstado(int ticketId, EstadoTicket novoEstado, SubEstadoAtendimento? subEstado, int tecnicoId, object? detalhes = null);

        /// <summary>
        /// Obtém um ticket pelo seu ID.
        /// </summary>
        Ticket ObterPorId(int id);

        /// <summary>
        /// Lista todos os tickets.
        /// </summary>
        List<Ticket> ObterTodos();

        /// <summary>
        /// Lista todos os tickets com determinado estado.
        /// </summary>
        List<Ticket> ObterPorEstado(EstadoTicket estado);
    }
}
