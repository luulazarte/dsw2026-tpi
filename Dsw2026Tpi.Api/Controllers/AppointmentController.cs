using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers
{
   
        [ApiController]
        [Route("api/appointments")]
        [Authorize]
    public class AppointmentsController : AppController
        {
            private readonly IAppointmentService _appointmentService;

            public AppointmentsController(IAppointmentService appointmentService)
            {
                _appointmentService = appointmentService;
            }

            [HttpPost]
            [Authorize(Policy = Policies.PatientPolicy)]
            [EnableRateLimiting("BookingPolicy")]
        public async Task<IActionResult> Create([FromBody] AppointmentModel.Request request)
        {
            
                var result = await _appointmentService.Create(request);
                return Ok(result);
            
           
        }

        [HttpGet("patient")]
        [Authorize(Policy = Policies.PatientPolicy)]
        public async Task<IActionResult> GetPatientAppointments([FromQuery] long dni)
            {
            var result = await _appointmentService.GetByPatientDni(dni);
            return Ok(result);
            }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Policies.PatientPolicy)]
        public async Task<IActionResult> Cancel(Guid id)
            {
                
                    await _appointmentService.Cancel(id);
                    return Ok("ok");
                
                
            }

            [HttpGet]
            [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
            {
                var result = await _appointmentService.GetAppointmentsByDate(date);
                return Ok(result);
            }

            [HttpGet("search")]
            [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> Search([FromQuery] Guid? specialtyId, [FromQuery] Guid? doctorId, [FromQuery] long? dni, [FromQuery] DateTime? date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
            {
            var result = await _appointmentService.SearchAppointments(specialtyId, doctorId, dni, date, pageSize, pageIndex);
            return Ok(result);
            }
        }
    }



