using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IPersistence _persistence;

        public AppointmentService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<Pagination<AppointmentModel.Response>> GetAll(int pageSize, int pageIndex, Guid? patientId = null)
        {
            var list = await _persistence.Paginate<Appointment, string>(
                pageSize,
                pageIndex,
                ap => !patientId.HasValue || ap.PatientId == patientId.Value,
                x => x.Reason
            );

            return list.Map(ap => new AppointmentModel.Response(ap.Id, ap.AvailabilitySlotId, ap.PatientId, ap.Reason, ap.Status));
        }

        public async Task<AppointmentModel.Response> Create(AppointmentModel.Request request)
        {
          
            var appointment = new Appointment(request.AvailabilitySlotId, request.PatientId, request.Reason, Guid.NewGuid());

            await _persistence.Add(appointment);

            return new AppointmentModel.Response(appointment.Id, appointment.AvailabilitySlotId, appointment.PatientId, appointment.Reason, appointment.Status);
        }

        public async Task Cancel(Guid id)
        {
            var appointment = await _persistence.GetById<Appointment>(id);
            if (appointment != null)
            {
             
                appointment.Cancel();
                await _persistence.Update(appointment);
            }
        }
    }
}