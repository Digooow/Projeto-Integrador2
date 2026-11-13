using FluentValidation;
using Projeto_Integrador2.Domain;
using Projeto_Integrador2.Models;

namespace Projeto_Integrador2.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe e-mail e senha.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Informe e-mail e senha.");
    }
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Informe nome, e-mail e senha.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe nome, e-mail e senha.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Informe nome, e-mail e senha.")
            .MinimumLength(8).WithMessage("A senha precisa ter pelo menos 8 caracteres.");
    }
}

public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.RequesterId)
            .NotEmpty().WithMessage("Informe o usuário solicitante.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Informe a sala da reserva.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Informe o título da reserva.");

        RuleFor(x => x.Attendees)
            .GreaterThan(0).WithMessage("Informe ao menos 1 participante.");

        RuleFor(x => x.End)
            .GreaterThan(x => x.Start)
            .WithMessage("O horário final precisa ser depois do inicial.");

        When(x => x.Recurrence is not null, () =>
        {
            RuleFor(x => x.Recurrence!.Days)
                .NotEmpty().WithMessage("Selecione ao menos um dia da semana.")
                .Must(days => days.All(d => d is >= 0 and <= 6))
                .WithMessage("Os dias da recorrência devem estar entre 0 (Domingo) e 6 (Sábado).");

            RuleFor(x => x.Recurrence!.Until.Date)
                .GreaterThanOrEqualTo(x => x.Start.Date)
                .WithMessage("A data limite da recorrência deve ser igual ou posterior à data inicial.");
        });
    }
}

public sealed class UpsertRoomRequestValidator : AbstractValidator<UpsertRoomRequest>
{
    public UpsertRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Informe nome, andar e capacidade (> 0).");

        RuleFor(x => x.Floor)
            .NotEmpty().WithMessage("Informe nome, andar e capacidade (> 0).");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Informe nome, andar e capacidade (> 0).");

        RuleSet("Create", () =>
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe id, nome, andar e capacidade (> 0).");
        });
    }
}

public sealed class UpsertResourceRequestValidator : AbstractValidator<UpsertResourceRequest>
{
    public UpsertResourceRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Informe id e nome do recurso.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Informe id e nome do recurso.");
    }
}

public sealed class UpsertUserRequestValidator : AbstractValidator<UpsertUserRequest>
{
    public UpsertUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Informe nome e e-mail.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe nome e e-mail.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Informe o papel do usuário.")
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage(x => $"Papel inválido: {x.Role}.");

        RuleSet("Create", () =>
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe id, nome e e-mail.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Informe uma senha.")
                .MinimumLength(8).WithMessage("A senha precisa ter pelo menos 8 caracteres.");
        });
    }
}
