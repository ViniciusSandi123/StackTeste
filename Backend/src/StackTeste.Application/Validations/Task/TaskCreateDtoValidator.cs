using FluentValidation;
using StackTeste.Application.DTOs.Task;

namespace StackTeste.Application.Validations.Task
{
    public class TaskCreateDtoValidator : AbstractValidator<TaskCreateDto>
    {
        public TaskCreateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O título é obrigatório.")
                .MaximumLength(200);

            RuleFor(x => x.Status!.Value)
                .IsInEnum().WithMessage("Status inválido.")
                .When(x => x.Status.HasValue);
        }
    }
}
