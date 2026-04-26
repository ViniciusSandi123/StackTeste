using TaskStatus = StackTeste.Domain.Helpers.Enums.TaskStatus;

namespace StackTeste.Application.DTOs.Task
{
    public record TaskUpdateDto(
        string Title,
        DateTime? DueDate,
        TaskStatus Status
    );
}
