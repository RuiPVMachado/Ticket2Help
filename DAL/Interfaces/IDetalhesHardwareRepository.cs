using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MODELS;

namespace DAL.Interfaces
{
    public interface IDetalhesHardwareRepository
    {
        int Criar(DetalhesHardware detalhes);
        DetalhesHardware ObterPorTicketId(int ticketId);
        bool Atualizar(DetalhesHardware detalhes);
    }
}