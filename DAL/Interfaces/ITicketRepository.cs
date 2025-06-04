using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MODELS;
using System.Collections.Generic;

namespace DAL.Interfaces
{
    public interface ITicketRepository
    {
        int Criar(Ticket ticket);
        Ticket ObterPorId(int id);
        List<Ticket> ObterTodos();
        List<Ticket> ObterPorEstado(EstadoTicket estado);
        bool AtualizarEstado(int ticketId, EstadoTicket novoEstado, SubEstadoAtendimento? subEstado, int? tecnicoId = null);
        bool Eliminar(int ticketId);
    }
}

