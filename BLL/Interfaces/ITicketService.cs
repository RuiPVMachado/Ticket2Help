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
        /// Cria um novo ticket de hardware utilizando o factory pattern.
        /// </summary>
        /// <param name="titulo">Título do ticket</param>
        /// <param name="descricao">Descrição geral do problema</param>
        /// <param name="prioridade">Prioridade do ticket</param>
        /// <param name="colaboradorId">ID do colaborador que cria o ticket</param>
        /// <param name="equipamento">Equipamento com problema</param>
        /// <param name="avaria">Descrição da avaria</param>
        /// <returns>ID do ticket criado</returns>
        int CriarTicketHardware(string titulo, string descricao, Prioridade prioridade,
            int colaboradorId, string equipamento, string avaria);

        /// <summary>
        /// Cria um novo ticket de software utilizando o factory pattern.
        /// </summary>
        /// <param name="titulo">Título do ticket</param>
        /// <param name="descricao">Descrição geral da necessidade</param>
        /// <param name="prioridade">Prioridade do ticket</param>
        /// <param name="colaboradorId">ID do colaborador que cria o ticket</param>
        /// <param name="aplicacao">Software/aplicação em questão</param>
        /// <param name="necessidade">Descrição da necessidade</param>
        /// <returns>ID do ticket criado</returns>
        int CriarTicketSoftware(string titulo, string descricao, Prioridade prioridade,
            int colaboradorId, string aplicacao, string necessidade);

        /// <summary>
        /// Cria um novo ticket (método genérico mantido para compatibilidade).
        /// </summary>
        /// <param name="ticket">Objeto base do ticket.</param>
        /// <param name="detalhes">Detalhes específicos (hardware/software).</param>
        /// <returns>Id do ticket criado.</returns>
        [Obsolete("Use CriarTicketHardware ou CriarTicketSoftware com o factory pattern")]
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
        /// <summary>
        /// Elimina um ticket e todos os seus detalhes associados.
        /// </summary>
        /// <param name="ticketId">ID do ticket a eliminar.</param>
        /// <returns>True se foi eliminado com sucesso.</returns>
        bool EliminarTicket(int ticketId);
    }
}