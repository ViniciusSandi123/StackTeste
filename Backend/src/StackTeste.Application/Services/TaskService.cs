using AutoMapper;
using FluentValidation;
using StackTeste.Application.DTOs.Task;
using StackTeste.Application.Interfaces;
using StackTeste.Domain.Common;
using StackTeste.Domain.Interfaces;
using StackTeste.Domain.Models;

namespace StackTeste.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILeadRepository _leadRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<TaskCreateDto> _createValidator;
        private readonly IValidator<TaskUpdateDto> _updateValidator;

        public TaskService(
            ITaskRepository taskRepository,
            ILeadRepository leadRepository,
            IMapper mapper,
            IValidator<TaskCreateDto> createValidator,
            IValidator<TaskUpdateDto> updateValidator)
        {
            _taskRepository = taskRepository;
            _leadRepository = leadRepository;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<IEnumerable<TaskDto>>> GetByLeadAsync(int leadId, CancellationToken ct = default)
        {
            var existe = await _leadRepository.ExistsAsync(leadId, ct);
            if (!existe)
            {
                return Result<IEnumerable<TaskDto>>.Fail($"Lead com Id {leadId} não foi encontrado.");
            }

            var tasks = await _taskRepository.GetByLeadAsync(leadId, ct);
            var retorno = _mapper.Map<IEnumerable<TaskDto>>(tasks);
            return Result<IEnumerable<TaskDto>>.Ok(retorno);
        }

        public async Task<Result<TaskDto>> GetByIdAsync(int leadId, int taskId, CancellationToken ct = default)
        {
            var task = await _taskRepository.GetByIdAsync(leadId, taskId, ct);
            if (task == null)
            {
                return Result<TaskDto>.Fail($"Task {taskId} não foi encontrada para o Lead {leadId}.");
            }

            var retorno = _mapper.Map<TaskDto>(task);
            return Result<TaskDto>.Ok(retorno);
        }

        public async Task<Result<TaskDto>> CreateAsync(int leadId, TaskCreateDto dto, CancellationToken ct = default)
        {
            await _createValidator.ValidateAndThrowAsync(dto, ct);
            var existe = await _leadRepository.ExistsAsync(leadId, ct);

            if (!existe)
            {
                return Result<TaskDto>.Fail($"Lead com Id {leadId} não foi encontrado.");
            }

            var task = _mapper.Map<TaskItem>(dto);
            task.LeadId = leadId;

            await _taskRepository.AddAsync(task, ct);
            var retorno = _mapper.Map<TaskDto>(task);

            return Result<TaskDto>.Ok(retorno);
        }

        public async Task<Result<TaskDto>> UpdateAsync(int leadId, int taskId, TaskUpdateDto dto, CancellationToken ct = default)
        {
            await _updateValidator.ValidateAndThrowAsync(dto, ct);

            var task = await _taskRepository.GetByIdAsync(leadId, taskId, ct);
            if (task == null)
            {
                return Result<TaskDto>.Fail($"Task {taskId} não foi encontrada para o Lead {leadId}.");
            }

            _mapper.Map(dto, task);
            await _taskRepository.UpdateAsync(task, ct);
            var retorno = _mapper.Map<TaskDto>(task);

            return Result<TaskDto>.Ok(retorno);
        }

        public async Task<Result> DeleteAsync(int leadId, int taskId, CancellationToken ct = default)
        {
            var task = await _taskRepository.GetByIdAsync(leadId, taskId, ct);
            if (task == null)
            {
                return Result.Fail($"Task {taskId} não foi encontrada para o Lead {leadId}.");
            }

            await _taskRepository.DeleteAsync(task, ct);
            return Result.Ok();
        }
    }
}
