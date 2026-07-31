using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("api/specialties")]
    [ApiController]
    [Authorize]
    public class SpecialtiesController : AppController
    {
        private readonly ISpecialityService _specialityService;

        public SpecialtiesController(ISpecialityService specialityService)
        {
            _specialityService = specialityService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, [FromQuery] string? name = null)
        {
            var result = await _specialityService.GetAll(pageSize, pageIndex, name);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Policies.AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SpecialityModel.Request request)
        {
            var result = await _specialityService.Create(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SpecialityModel.Request request)
        {
            var result = await _specialityService.Update(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _specialityService.Delete(id);
            return NoContent();
        }

    }
}