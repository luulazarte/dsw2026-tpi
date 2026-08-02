using Dsw2026Tpi.Domain.Enums;
using System;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentModel
    {

        public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);

        public record PatientDto(long Dni);

      
        public record PatientResponse(Guid Id, DateTime Date, TimeSpan StartTime, string DoctorName, string SpecialtyName, AppointmentStatus Status);


        public record SearchResponse(
              Guid AppointmentsId,
              string AppointmentsStatus,
              SearchPatientDto Patient,
              SearchDoctorDto Doctor);

        public record SearchPatientDto(string Dni, string FullName);

        public record SearchDoctorDto(Guid DoctorId, string Name, SearchSpecialtyDto Specialty);

        public record SearchSpecialtyDto(Guid SpecialtyId, string Name);
    }
}