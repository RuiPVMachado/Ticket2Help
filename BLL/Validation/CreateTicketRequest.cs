using MODELS;

namespace BLL.Validation
{
    /// <summary>
    /// Request para criação de ticket de hardware
    /// </summary>
    public class CreateHardwareTicketRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public Prioridade Prioridade { get; set; }
        public int ColaboradorId { get; set; }
        public string Equipamento { get; set; } = string.Empty;
        public string Avaria { get; set; } = string.Empty;
        public int CriadoPorId { get; set; }
        public bool CriadoPorTecnico { get; set; }
    }

    /// <summary>
    /// Request para criação de ticket de software
    /// </summary>
    public class CreateSoftwareTicketRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public Prioridade Prioridade { get; set; }
        public int ColaboradorId { get; set; }
        public string Aplicacao { get; set; } = string.Empty;
        public string Necessidade { get; set; } = string.Empty;
        public int CriadoPorId { get; set; }
        public bool CriadoPorTecnico { get; set; }
    }

    /// <summary>
    /// Request para atualização de estado do ticket
    /// </summary>
    public class UpdateTicketStateRequest
    {
        public int TicketId { get; set; }
        public EstadoTicket NovoEstado { get; set; }
        public int TecnicoId { get; set; }
    }

    /// <summary>
    /// Request para eliminação de ticket
    /// </summary>
    public class DeleteTicketRequest
    {
        public int TicketId { get; set; }
        public int SolicitadoPorId { get; set; }
        public bool SolicitadoPorTecnico { get; set; }
    }
}