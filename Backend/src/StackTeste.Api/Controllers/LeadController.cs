using Microsoft.AspNetCore.Mvc;
using StackTeste.Application.DTOs.Lead;
using StackTeste.Application.Interfaces;
using StackTeste.Domain.Common;
using StackTeste.Domain.Helpers.Enums;

namespace StackTeste.Api.Controllers
{
    [ApiController]
    [Route("api/leads")]
    [Produces("application/json")]
    public class LeadController : ControllerBase
    {
        private readonly ILeadService _leadService;

        public LeadController(ILeadService leadService)
        {
            _leadService = leadService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<LeadDto>>> Get(
            [FromQuery] string? search,
            [FromQuery] LeadStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var paged = await _leadService.GetAllAsync(search, status, page, pageSize, ct);

            return Ok(paged);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LeadDetailDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _leadService.GetByIdAsync(id, ct);

            if (!result.Success)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<ActionResult<LeadDto>> Create([FromBody] LeadCreateDto dto, CancellationToken ct)
        {
            var result = await _leadService.CreateAsync(dto, ct);

            if (!result.Success)
            {
                return BadRequest(result.Error);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<LeadDto>> Update(int id, [FromBody] LeadUpdateDto dto, CancellationToken ct)
        {
            var result = await _leadService.UpdateAsync(id, dto, ct);

            if (!result.Success)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _leadService.DeleteAsync(id, ct);

            if (!result.Success)
            {
                return NotFound(result.Error);
            }

            return NoContent();
        }
    }
}