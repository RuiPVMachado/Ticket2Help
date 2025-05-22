namespace MODELS
{
    public class DetalhesHardware
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Equipamento { get; set; }
        public string Avaria { get; set; }
        public string? PecaSubstituida { get; set; }
        public string? DescricaoReparacao { get; set; }
    }
}
