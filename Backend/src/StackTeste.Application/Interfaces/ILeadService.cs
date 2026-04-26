using StackTeste.Application.DTOs.Lead;
using StackTeste.Domain.Common;
using StackTeste.Domain.Helpers.Enums;

namespace StackTeste.Application.Interfaces
{
    public interface ILeadService
    {
        Task<PagedResult<LeadDto>> GetAllAsync(
            string? search,
            LeadStatus? status,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<Result<LeadDetailDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<LeadDto>> CreateAsync(LeadCreateDto dto, CancellationToken ct = default);
        Task<Result<LeadDto>> UpdateAsync(int id, LeadUpdateDto dto, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    }
}
