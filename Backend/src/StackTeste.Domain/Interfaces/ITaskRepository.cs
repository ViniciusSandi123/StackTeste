using StackTeste.Domain.Models;
using TaskCountByLeadId = System.Collections.Generic.IReadOnlyDictionary<int, int>;

namespace StackTeste.Domain.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetByLeadAsync(int leadId, CancellationToken ct = default);
        Task<TaskCountByLeadId> GetCountsByLeadIdsAsync(IEnumerable<int> leadIds, CancellationToken ct = default);
        Task<TaskItem?> GetByIdAsync(int leadId, int taskId, CancellationToken ct = default);
        Task<TaskItem> AddAsync(TaskItem task, CancellationToken ct = default);
        Task UpdateAsync(TaskItem task, CancellationToken ct = default);
        Task DeleteAsync(TaskItem task, CancellationToken ct = default);
    }
}
