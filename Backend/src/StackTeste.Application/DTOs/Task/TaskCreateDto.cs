using TaskStatus = StackTeste.Domain.Helpers.Enums.TaskStatus;

namespace StackTeste.Application.DTOs.Task
{
    public record TaskCreateDto(
        string Title,
        DateTime? DueDate,
        TaskStatus? Status
    );
}
