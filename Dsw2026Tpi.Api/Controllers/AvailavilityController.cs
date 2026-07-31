using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/availabilities")]
[Authorize(Policy = Policies.AdminPolicy)]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AvailabilityModel.Request request)
    {
        await _availabilityService.Create(request);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AvailabilityModel.Request request)
    {
        await _availabilityService.Update(request);
        return Ok();
    }
}