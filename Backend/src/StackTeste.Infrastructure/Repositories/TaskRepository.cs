using Microsoft.EntityFrameworkCore;
using StackTeste.Domain.Interfaces;
using StackTeste.Domain.Models;
using StackTeste.Infrastructure.Data;
using TaskCountByLeadId = System.Collections.Generic.IReadOnlyDictionary<int, int>;

namespace StackTeste.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly Context _context;

        public TaskRepository(Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskItem>> GetByLeadAsync(int leadId, CancellationToken ct = default)
        {
            return await _context.TaskItens
                .AsNoTracking()
                .Where(t => t.LeadId == leadId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<TaskCountByLeadId> GetCountsByLeadIdsAsync(IEnumerable<int> leadId, CancellationToken ct = default)
        {
            var ids = leadId.ToList();
            return await _context.TaskItens
                .AsNoTracking()
                .Where(t => ids.Contains(t.LeadId))
                .GroupBy(t => t.LeadId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        }

        public Task<TaskItem?> GetByIdAsync(int leadId, int taskId, CancellationToken ct = default)
        {
            return _context.TaskItens
                .FirstOrDefaultAsync(t => t.Id == taskId && t.LeadId == leadId, ct);
        }

        public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken ct = default)
        {
            await _context.TaskItens.AddAsync(task, ct);
            await _context.SaveChangesAsync(ct);
            return task;
        }

        public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
        {
            _context.TaskItens.Update(task);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(TaskItem task, CancellationToken ct = default)
        {
            _context.TaskItens.Remove(task);
            await _context.SaveChangesAsync(ct);
        }
    }
}
