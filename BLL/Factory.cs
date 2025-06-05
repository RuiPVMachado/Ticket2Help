using MODELS;
using System;

namespace BLL
{
    /// <summary>
    /// Factory responsável pela criação de tickets seguindo o padrão Factory.
    /// Centraliza a lógica de criação e garante consistência na inicialização.
    /// </summary>
    public static class TicketFactory
    {
        /// <summary>
        /// Cria um novo ticket com os valores padrão definidos.
        /// </summary>
        /// <param name="tipo">Tipo do ticket (Hardware ou Software)</param>
        /// <param name="titulo">Título do ticket</param>
        /// <param name="descricao">Descrição do problema/necessidade</param>
        /// <param name="prioridade">Prioridade do ticket</param>
        /// <param name="colaboradorId">ID do colaborador que tem o problema</param>
        /// <param name="criadoPorId">ID do utilizador que está a criar o ticket (pode ser técnico ou colaborador)</param>
        /// <param name="criadoPorTecnico">Indica se foi criado por técnico (padrão: false)</param>
        /// <returns>Novo ticket inicializado</returns>
        public static Ticket CriarTicket(
            TipoTicket tipo,
            string titulo,
            string descricao,
            Prioridade prioridade,
            int colaboradorId,
            int criadoPorId,
            bool criadoPorTecnico = false)
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("Título não pode estar vazio", nameof(titulo));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição não pode estar vazia", nameof(descricao));

            if (colaboradorId <= 0)
                throw new ArgumentException("ID do colaborador deve ser válido", nameof(colaboradorId));

            if (criadoPorId <= 0)
                throw new ArgumentException("ID de quem cria deve ser válido", nameof(criadoPorId));

            return new Ticket
            {
                Tipo = tipo,
                Titulo = titulo,
                Descricao = descricao,
                Prioridade = prioridade,
                IdColaborador = colaboradorId,
                // Se criado por técnico, já pode ficar em atendimento e ter técnico atribuído
                Estado = criadoPorTecnico ? EstadoTicket.EmAtendimento : EstadoTicket.PorAtender,
                DataCriacao = DateTime.Now,
                DataAtendimento = criadoPorTecnico ? DateTime.Now : (DateTime?)null,
                IdTecnico = criadoPorTecnico ? criadoPorId : (int?)null,
                SubEstado = null
            };
        }

        /// <summary>
        /// Cria um ticket de hardware com detalhes específicos.
        /// </summary>
        /// <param name="titulo">Título do ticket</param>
        /// <param name="descricao">Descrição geral</param>
        /// <param name="prioridade">Prioridade</param>
        /// <param name="colaboradorId">ID do colaborador com o problema</param>
        /// <param name="equipamento">Equipamento com problema</param>
        /// <param name="avaria">Descrição da avaria</param>
        /// <param name="criadoPorId">ID de quem está a criar o ticket</param>
        /// <param name="criadoPorTecnico">Se foi criado por técnico</param>
        /// <returns>Tupla com ticket e detalhes de hardware</returns>
        public static (Ticket ticket, DetalhesHardware detalhes) CriarTicketHardware(
            string titulo,
            string descricao,
            Prioridade prioridade,
            int colaboradorId,
            string equipamento,
            string avaria,
            int criadoPorId,
            bool criadoPorTecnico = false)
        {
            var ticket = CriarTicket(TipoTicket.Hardware, titulo, descricao, prioridade,
                                   colaboradorId, criadoPorId, criadoPorTecnico);

            var detalhes = new DetalhesHardware
            {
                Equipamento = equipamento ?? throw new ArgumentException("Equipamento não pode estar vazio"),
                Avaria = avaria ?? throw new ArgumentException("Avaria não pode estar vazia"),
                // Campos preenchidos durante atendimento
                PecaSubstituida = null,
                DescricaoReparacao = null
            };

            return (ticket, detalhes);
        }

        /// <summary>
        /// Cria um ticket de software com detalhes específicos.
        /// </summary>
        /// <param name="titulo">Título do ticket</param>
        /// <param name="descricao">Descrição geral</param>
        /// <param name="prioridade">Prioridade</param>
        /// <param name="colaboradorId">ID do colaborador com a necessidade</param>
        /// <param name="aplicacao">Software/aplicação em questão</param>
        /// <param name="necessidade">Descrição da necessidade</param>
        /// <param name="criadoPorId">ID de quem está a criar o ticket</param>
        /// <param name="criadoPorTecnico">Se foi criado por técnico</param>
        /// <returns>Tupla com ticket e detalhes de software</returns>
        public static (Ticket ticket, DetalhesSoftware detalhes) CriarTicketSoftware(
            string titulo,
            string descricao,
            Prioridade prioridade,
            int colaboradorId,
            string aplicacao,
            string necessidade,
            int criadoPorId,
            bool criadoPorTecnico = false)
        {
            var ticket = CriarTicket(TipoTicket.Software, titulo, descricao, prioridade,
                                   colaboradorId, criadoPorId, criadoPorTecnico);

            var detalhes = new DetalhesSoftware
            {
                Aplicacao = aplicacao ?? throw new ArgumentException("Aplicação não pode estar vazia"),
                DescricaoNecessidade = necessidade ?? throw new ArgumentException("Necessidade não pode estar vazia"),
                // Campo preenchido durante atendimento
                SolucaoAplicada = null
            };

            return (ticket, detalhes);
        }

        /// <summary>
        /// Métodos de compatibilidade para criação por colaboradores (sem mudanças na assinatura existente)
        /// </summary>
        public static Ticket CriarTicket(
            TipoTicket tipo,
            string titulo,
            string descricao,
            Prioridade prioridade,
            int colaboradorId)
        {
            return CriarTicket(tipo, titulo, descricao, prioridade, colaboradorId, colaboradorId, false);
        }

        public static (Ticket ticket, DetalhesHardware detalhes) CriarTicketHardware(
            string titulo,
            string descricao,
            Prioridade prioridade,
            int colaboradorId,
            string equipamento,
            string avaria)
        {
            return CriarTicketHardware(titulo, descricao, prioridade, colaboradorId,
                                     equipamento, avaria, colaboradorId, false);
        }

        public static (Ticket ticket, DetalhesSoftware detalhes) CriarTicketSoftware(
            string titulo,
            string descricao,
            Prioridade prioridade,
            int colaboradorId,
            string aplicacao,
            string necessidade)
        {
            return CriarTicketSoftware(titulo, descricao, prioridade, colaboradorId,
                                     aplicacao, necessidade, colaboradorId, false);
        }
    }
}