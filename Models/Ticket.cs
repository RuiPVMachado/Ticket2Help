namespace MODELS
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public Prioridade Prioridade { get; set; }
        public TipoTicket Tipo { get; set; }
        public EstadoTicket Estado { get; set; }
        public SubEstadoAtendimento? SubEstado { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtendimento { get; set; }
        public int IdColaborador { get; set; }
        public int? IdTecnico { get; set; }
    }
}
