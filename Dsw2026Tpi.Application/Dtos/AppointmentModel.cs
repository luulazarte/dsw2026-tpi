using Dsw2026Tpi.Domain.Enums;
using System;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentModel
    {

        public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);

        public record PatientDto(long Dni);

      
        public record PatientResponse(Guid Id, DateTime Date, TimeSpan StartTime, string DoctorName, string SpecialtyName, AppointmentStatus Status);

    
        public record SearchResponse(string Specialty, string Doctor, DateTime AvailableDate, TimeSpan AvailableTime);
    }
}