using StackTeste.Domain.Common;
using StackTeste.Domain.Helpers.Enums;
using StackTeste.Domain.Models;

namespace StackTeste.Domain.Interfaces
{
    public interface ILeadRepository
    {
        Task<PagedResult<Lead>> GetAllAsync(string? search, LeadStatus? status, int page, int pageSize, CancellationToken ct = default);
        Task<Lead?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Lead?> GetByIdWithTasksAsync(int id, CancellationToken ct = default);
        Task<Lead> AddAsync(Lead lead, CancellationToken ct = default);
        Task UpdateAsync(Lead lead, CancellationToken ct = default);
        Task DeleteAsync(Lead lead, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    }
}
