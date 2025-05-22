namespace MODELS
{
    public class DetalhesSoftware
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Aplicacao { get; set; }
        public string DescricaoNecessidade { get; set; }
        public string? SolucaoAplicada { get; set; }
    }
}
