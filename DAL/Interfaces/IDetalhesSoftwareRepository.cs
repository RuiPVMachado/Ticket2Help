using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MODELS;

namespace DAL.Interfaces
{
    public interface IDetalhesSoftwareRepository
    {
        int Criar(DetalhesSoftware detalhes);
        DetalhesSoftware ObterPorTicketId(int ticketId);
        bool Atualizar(DetalhesSoftware detalhes);
    }
}
