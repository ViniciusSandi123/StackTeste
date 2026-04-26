using StackTeste.Application.DTOs.Task;
using StackTeste.Domain.Helpers.Enums;

namespace StackTeste.Application.DTOs.Lead
{
    public record LeadDetailDto(
        int Id,
        string Name,
        string Email,
        LeadStatus Status,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        IEnumerable<TaskDto> Tasks
    );
}
