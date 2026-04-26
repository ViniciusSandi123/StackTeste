using Microsoft.EntityFrameworkCore;
using StackTeste.Domain.Common;
using StackTeste.Domain.Helpers.Enums;
using StackTeste.Domain.Interfaces;
using StackTeste.Domain.Models;
using StackTeste.Infrastructure.Data;

namespace StackTeste.Infrastructure.Repositories
{
    public class LeadRepository : ILeadRepository
    {
        private const int MaxPageSize = 100;

        private readonly Context _context;

        public LeadRepository(Context context)
        {
            _context = context;
        }

        public async Task<PagedResult<Lead>> GetAllAsync(
            string? search,
            LeadStatus? status,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            if (page < 1) 
            {
                page = 1;
            }
            
            if (pageSize < 1)
            {
                pageSize = 10;
            }

            if (pageSize > MaxPageSize)
            {
                pageSize = MaxPageSize;
            }

            IQueryable<Lead> query = _context.Leads.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = $"%{search.Trim()}%";
                query = query.Where(l =>
                    EF.Functions.Like(l.Name, term) ||
                    EF.Functions.Like(l.Email, term));
            }

            if (status.HasValue)
            {
                query = query.Where(l => l.Status == status.Value);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Lead>(items, totalCount, page, pageSize);
        }

        public Task<Lead?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _context.Leads.FirstOrDefaultAsync(l => l.Id == id, ct);
        }

        public Task<Lead?> GetByIdWithTasksAsync(int id, CancellationToken ct = default)
        {
            return _context.Leads
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(l => l.Id == id, ct);
        }

        public async Task<Lead> AddAsync(Lead lead, CancellationToken ct = default)
        {
            await _context.Leads.AddAsync(lead, ct);
            await _context.SaveChangesAsync(ct);
            return lead;
        }

        public async Task UpdateAsync(Lead lead, CancellationToken ct = default)
        {
            _context.Leads.Update(lead);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Lead lead, CancellationToken ct = default)
        {
            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync(ct);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            return _context.Leads.AnyAsync(l => l.Id == id, ct);
        }
    }
}
