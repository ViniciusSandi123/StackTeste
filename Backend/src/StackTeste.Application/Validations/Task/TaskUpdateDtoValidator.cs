using FluentValidation;
using StackTeste.Application.DTOs.Task;

namespace StackTeste.Application.Validations.Task
{
    public class TaskUpdateDtoValidator : AbstractValidator<TaskUpdateDto>
    {
        public TaskUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O título é obrigatório.")
                .MaximumLength(200);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status inválido.");
        }
    }
}
