using TaskStatus = StackTeste.Domain.Helpers.Enums.TaskStatus;

namespace StackTeste.Application.DTOs.Task
{
    public record TaskDto(
        int Id,
        int LeadId,
        string Title,
        DateTime? DueDate,
        TaskStatus Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
