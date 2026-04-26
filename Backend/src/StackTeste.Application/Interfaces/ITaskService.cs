using StackTeste.Application.DTOs.Task;
using StackTeste.Domain.Common;

namespace StackTeste.Application.Interfaces
{
    public interface ITaskService
    {
        Task<Result<IEnumerable<TaskDto>>> GetByLeadAsync(int leadId, CancellationToken ct = default);
        Task<Result<TaskDto>> GetByIdAsync(int leadId, int taskId, CancellationToken ct = default);
        Task<Result<TaskDto>> CreateAsync(int leadId, TaskCreateDto dto, CancellationToken ct = default);
        Task<Result<TaskDto>> UpdateAsync(int leadId, int taskId, TaskUpdateDto dto, CancellationToken ct = default);
        Task<Result> DeleteAsync(int leadId, int taskId, CancellationToken ct = default);
    }
}
