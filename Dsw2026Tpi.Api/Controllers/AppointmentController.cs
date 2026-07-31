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
    public class AppointmentsController : ControllerBase
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
                try
                {
                    await _appointmentService.Create(request);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return ManejarErrorPersonalizado(ex);
                }
            }

            [HttpGet("patient")]
            [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> GetPatientAppointments([FromQuery] long dni)
            {
                var result = await _appointmentService.GetByPatientDni(dni);
                return Ok(result);
            }

            [HttpDelete("{id:guid}")]
            [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> Cancel(Guid id)
            {
                try
                {
                    await _appointmentService.Cancel(id);
                    return NoContent();
                }
                catch (Exception ex)
                {
                    return ManejarErrorPersonalizado(ex);
                }
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

          
            private IActionResult ManejarErrorPersonalizado(Exception ex)
            {
                var partes = ex.Message.Split('|');
                var errorType = partes.Length > 1 ? partes[0] : "SERVER_ERROR";
                var errorMsg = partes.Length > 1 ? partes[1] : ex.Message;

                var errorCode = errorType == "conflict" ? "APPOINTMENT_CONFLICT" : "BAD_REQUEST";

                var errorResponse = new
                {
                    errorCode = errorCode,
                    message = errorMsg,
                    details = new List<object>
                {
                    new { field = "appointment", issue = errorType == "conflict" ? "slot_unavailable" : "validation_failed" }
                }
                };

                return BadRequest(errorResponse);
            }
        }
    }



