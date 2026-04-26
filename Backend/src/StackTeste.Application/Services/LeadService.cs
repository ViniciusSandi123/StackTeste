using AutoMapper;
using FluentValidation;
using StackTeste.Application.DTOs.Lead;
using StackTeste.Application.Interfaces;
using StackTeste.Domain.Common;
using StackTeste.Domain.Helpers.Enums;
using StackTeste.Domain.Interfaces;
using StackTeste.Domain.Models;

namespace StackTeste.Application.Services
{
    public class LeadService : ILeadService
    {
        private readonly ILeadRepository _leadRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<LeadCreateDto> _createValidator;
        private readonly IValidator<LeadUpdateDto> _updateValidator;

        public LeadService(
            ILeadRepository leadRepository,
            ITaskRepository taskRepository,
            IMapper mapper,
            IValidator<LeadCreateDto> createValidator,
            IValidator<LeadUpdateDto> updateValidator)
        {
            _leadRepository = leadRepository;
            _taskRepository = taskRepository;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PagedResult<LeadDto>> GetAllAsync(
            string? search,
            LeadStatus? status,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var paged = await _leadRepository.GetAllAsync(search, status, page, pageSize, ct);
            var leadIds = paged.Items.Select(l => l.Id).ToList();
            var tasksByLead = await _taskRepository.GetCountsByLeadIdsAsync(leadIds, ct);

            var dtos = paged.Items.Select(lead =>
                _mapper.Map<LeadDto>(lead, opts => opts.Items["taskCounts"] = tasksByLead));

            return new PagedResult<LeadDto>(dtos, paged.TotalCount, paged.Page, paged.PageSize);
        }

        public async Task<Result<LeadDetailDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var lead = await _leadRepository.GetByIdWithTasksAsync(id, ct);
            if (lead == null)
            {
                return Result<LeadDetailDto>.Fail($"Lead com Id {id} não foi encontrado.");
            }
            var retorno = _mapper.Map<LeadDetailDto>(lead);

            return Result<LeadDetailDto>.Ok(retorno);
        }

        public async Task<Result<LeadDto>> CreateAsync(LeadCreateDto dto, CancellationToken ct = default)
        {
            await _createValidator.ValidateAndThrowAsync(dto, ct);

            var lead = _mapper.Map<Lead>(dto);
            await _leadRepository.AddAsync(lead, ct);
            var retorno = _mapper.Map<LeadDto>(lead);

            return Result<LeadDto>.Ok(retorno);
        }

        public async Task<Result<LeadDto>> UpdateAsync(int id, LeadUpdateDto dto, CancellationToken ct = default)
        {
            await _updateValidator.ValidateAndThrowAsync(dto, ct);

            var lead = await _leadRepository.GetByIdAsync(id, ct);
            if (lead == null)
            {
                return Result<LeadDto>.Fail($"Lead com Id {id} não foi encontrado.");
            }

            _mapper.Map(dto, lead);
            await _leadRepository.UpdateAsync(lead, ct);
            var retorno = _mapper.Map<LeadDto>(lead);

            return Result<LeadDto>.Ok(retorno);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var lead = await _leadRepository.GetByIdAsync(id, ct);
            if (lead == null)
            {
                return Result.Fail($"Lead com Id {id} não foi encontrado.");
            }

            await _leadRepository.DeleteAsync(lead, ct);
            return Result.Ok();
        }
    }
}
