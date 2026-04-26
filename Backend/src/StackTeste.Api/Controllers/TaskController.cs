using Microsoft.AspNetCore.Mvc;
using StackTeste.Application.DTOs.Task;
using StackTeste.Application.Interfaces;

namespace StackTeste.Api.Controllers
{
    [ApiController]
    [Route("api/leads/{leadId:int}/tasks")]
    [Produces("application/json")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetByLead(int leadId, CancellationToken ct)
        {
            var result = await _taskService.GetByLeadAsync(leadId, ct);

            if (!result.Success)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }

        [HttpGet("{taskId:int}")]
        public async Task<ActionResult<TaskDto>> GetById(int leadId, int taskId, CancellationToken ct)
        {
            var result = await _taskService.GetByIdAsync(leadId, taskId, ct);

            if (!result.Success)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> Create(int leadId, [FromBody] TaskCreateDto dto, CancellationToken ct)
        {
            var result = await _taskService.CreateAsync(leadId, dto, ct);

            if (!result.Success)
            {
                return NotFound(result.Error);
            }

            return CreatedAtAction(nameof(GetById), new { leadId, taskId = result.Value!.Id }, result.Value);
        }

        [HttpPut("{taskId:int}")]
        public async Task<ActionResult<TaskDto>> Update(int leadId, int taskId, [FromBody] TaskUpdateDto dto, CancellationToken ct)
        {
            var result = await _taskService.UpdateAsync(leadId, taskId, dto, ct);

            if (!result.Success)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }

        [HttpDelete("{taskId:int}")]
        public async Task<IActionResult> Delete(int leadId, int taskId, CancellationToken ct)
        {
            var result = await _taskService.DeleteAsync(leadId, taskId, ct);

            if (!result.Success)
            {
                return NotFound(result.Error);
            }

            return NoContent();
        }
    }
}