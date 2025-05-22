namespace MODELS
{
    public class Utilizador
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public TipoUtilizador Tipo { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
