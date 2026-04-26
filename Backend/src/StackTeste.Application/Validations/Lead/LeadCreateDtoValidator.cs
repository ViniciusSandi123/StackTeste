using FluentValidation;
using StackTeste.Application.DTOs.Lead;

namespace StackTeste.Application.Validations.Lead
{
    public class LeadCreateDtoValidator : AbstractValidator<LeadCreateDto>
    {
        public LeadCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter ao menos 3 caracteres.")
                .MaximumLength(150);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(200);

            RuleFor(x => x.Status!.Value)
                .IsInEnum().WithMessage("Status inválido.")
                .When(x => x.Status.HasValue);
        }
    }
}
