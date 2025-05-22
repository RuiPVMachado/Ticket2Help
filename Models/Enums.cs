namespace MODELS
{
    public enum TipoUtilizador
    {
        Tecnico,
        Colaborador
    }

    public enum Prioridade
    {
        Alta,
        Media,
        Baixa
    }

    public enum TipoTicket
    {
        Hardware,
        Software
    }

    public enum EstadoTicket
    {
        PorAtender,
        EmAtendimento,
        Atendido
    }

    public enum SubEstadoAtendimento
    {
        Resolvido,
        NaoResolvido
    }
}
