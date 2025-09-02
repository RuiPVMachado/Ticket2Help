using FluentValidation;
using MODELS;

namespace BLL.Validation
{
    /// <summary>
    /// Validador para criação de tickets de hardware
    /// </summary>
    public class CreateHardwareTicketValidator : AbstractValidator<CreateHardwareTicketRequest>
    {
        public CreateHardwareTicketValidator()
        {
            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("Título é obrigatório")
                .Length(3, 200).WithMessage("Título deve ter entre 3 e 200 caracteres")
                .Matches(@"^[a-zA-Z0-9\s\-_.,!?]+$").WithMessage("Título contém caracteres inválidos");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("Descrição é obrigatória")
                .Length(10, 1000).WithMessage("Descrição deve ter entre 10 e 1000 caracteres");

            RuleFor(x => x.Prioridade)
                .IsInEnum().WithMessage("Prioridade inválida");

            RuleFor(x => x.ColaboradorId)
                .GreaterThan(0).WithMessage("ID do colaborador deve ser maior que zero");

            RuleFor(x => x.Equipamento)
                .NotEmpty().WithMessage("Equipamento é obrigatório")
                .Length(2, 100).WithMessage("Equipamento deve ter entre 2 e 100 caracteres");

            RuleFor(x => x.Avaria)
                .NotEmpty().WithMessage("Descrição da avaria é obrigatória")
                .Length(5, 500).WithMessage("Descrição da avaria deve ter entre 5 e 500 caracteres");

            RuleFor(x => x.CriadoPorId)
                .GreaterThan(0).WithMessage("ID do criador deve ser maior que zero");
        }
    }

    /// <summary>
    /// Validador para criação de tickets de software
    /// </summary>
    public class CreateSoftwareTicketValidator : AbstractValidator<CreateSoftwareTicketRequest>
    {
        public CreateSoftwareTicketValidator()
        {
            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("Título é obrigatório")
                .Length(3, 200).WithMessage("Título deve ter entre 3 e 200 caracteres")
                .Matches(@"^[a-zA-Z0-9\s\-_.,!?]+$").WithMessage("Título contém caracteres inválidos");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("Descrição é obrigatória")
                .Length(10, 1000).WithMessage("Descrição deve ter entre 10 e 1000 caracteres");

            RuleFor(x => x.Prioridade)
                .IsInEnum().WithMessage("Prioridade inválida");

            RuleFor(x => x.ColaboradorId)
                .GreaterThan(0).WithMessage("ID do colaborador deve ser maior que zero");

            RuleFor(x => x.Aplicacao)
                .NotEmpty().WithMessage("Aplicação é obrigatória")
                .Length(2, 100).WithMessage("Aplicação deve ter entre 2 e 100 caracteres");

            RuleFor(x => x.Necessidade)
                .NotEmpty().WithMessage("Descrição da necessidade é obrigatória")
                .Length(5, 500).WithMessage("Descrição da necessidade deve ter entre 5 e 500 caracteres");

            RuleFor(x => x.CriadoPorId)
                .GreaterThan(0).WithMessage("ID do criador deve ser maior que zero");
        }
    }

    /// <summary>
    /// Validador para atualização de estado do ticket
    /// </summary>
    public class UpdateTicketStateValidator : AbstractValidator<UpdateTicketStateRequest>
    {
        public UpdateTicketStateValidator()
        {
            RuleFor(x => x.TicketId)
                .GreaterThan(0).WithMessage("ID do ticket deve ser maior que zero");

            RuleFor(x => x.NovoEstado)
                .IsInEnum().WithMessage("Estado do ticket inválido");

            RuleFor(x => x.TecnicoId)
                .GreaterThan(0).WithMessage("ID do técnico deve ser maior que zero");

            // Regra de negócio: validação de transições de estado
            RuleFor(x => x.NovoEstado)
                .Must(BeValidStateTransition)
                .WithMessage("Transição de estado inválida");
        }

        private bool BeValidStateTransition(EstadoTicket estado)
        {
            // Implementação básica - pode ser expandida conforme regras de negócio
            return Enum.IsDefined(typeof(EstadoTicket), estado);
        }
    }

    /// <summary>
    /// Validador para eliminação de ticket
    /// </summary>
    public class DeleteTicketValidator : AbstractValidator<DeleteTicketRequest>
    {
        public DeleteTicketValidator()
        {
            RuleFor(x => x.TicketId)
                .GreaterThan(0).WithMessage("ID do ticket deve ser maior que zero");

            RuleFor(x => x.SolicitadoPorId)
                .GreaterThan(0).WithMessage("ID do solicitante deve ser maior que zero");
        }
    }

    /// <summary>
    /// Validador genérico para IDs
    /// </summary>
    public class IdValidator : AbstractValidator<int>
    {
        public IdValidator()
        {
            RuleFor(x => x)
                .GreaterThan(0).WithMessage("ID deve ser maior que zero");
        }
    }
}