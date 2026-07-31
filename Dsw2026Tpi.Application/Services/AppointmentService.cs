using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
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

        public async Task Create(AppointmentModel.Request request)
        {
           
            var dniStr = request.Patient.Dni.ToString();
            if (dniStr.Length < 7 || dniStr.Length > 10)
                throw new Exception("validation_failed|El DNI es obligatorio y debe tener entre 7 y 10 dígitos.");

            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
                throw new Exception("validation_failed|El motivo debe tener al menos 5 caracteres.");

         
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null || doctor.Deleted)
                throw new Exception("validation_failed|El doctor especificado no existe.");

            var patient = await _persistence.First<Patient>(p => p.Dni == dniStr);
            if (patient == null)
                throw new Exception("validation_failed|No existe un paciente registrado con ese DNI.");

  
            var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilityId);
            if (slot == null)
                throw new Exception("validation_failed|El slot no existe.");

            if (slot.SlotDate < DateTime.Today || (slot.SlotDate == DateTime.Today && slot.StartTime < DateTime.Now.TimeOfDay))
                throw new Exception("validation_failed|No se permiten turnos en el pasado.");

            if (slot.Status != SlotStatus.AVAILABLE)
                throw new Exception("conflict|Slot already booked"); 

            var appointment = new Appointment(slot.Id, patient.Id, request.Reason);
            await _persistence.Add(appointment);

 
            slot.Book();
            await _persistence.Update(slot);
        }

        public async Task Cancel(Guid id)
        {
            var appointment = await _persistence.GetById<Appointment>(id);
            if (appointment == null)
                throw new Exception("validation_failed|Turno no encontrado.");

            if (appointment.Status != AppointmentStatus.BOOKED)
                throw new Exception("validation_failed|Solo se puede cancelar un turno en estado BOOKED.");

            appointment.Cancel();
            await _persistence.Update(appointment);

            var slot = await _persistence.GetById<AvailabilitySlot>(appointment.AvailabilitySlotId);
            if (slot != null)
            {
                slot.Release();
                await _persistence.Update(slot);
            }
        }

        public async Task<IEnumerable<AppointmentModel.PatientResponse>> GetByPatientDni(long dni)
        {
            var dniStr = dni.ToString();
            var patient = await _persistence.First<Patient>(p => p.Dni == dniStr);
            if (patient == null) return new List<AppointmentModel.PatientResponse>();

            var appointments = await _persistence.GetFiltered<Appointment>(
                a => a.PatientId == patient.Id && a.Status == AppointmentStatus.BOOKED,
                "AvailabilitySlot", "AvailabilitySlot.AvailabilityRule.Doctor.Speciality"
            );

            if (appointments == null) return new List<AppointmentModel.PatientResponse>();

            var activos = appointments.Where(a =>
                a.AvailabilitySlot != null &&
                (a.AvailabilitySlot.SlotDate > DateTime.Today || (a.AvailabilitySlot.SlotDate == DateTime.Today && a.AvailabilitySlot.StartTime >= DateTime.Now.TimeOfDay))
            );

            return activos.Select(a => new AppointmentModel.PatientResponse(
                a.Id,
                a.AvailabilitySlot.SlotDate,
                a.AvailabilitySlot.StartTime,
                a.AvailabilitySlot.AvailabilityRule?.Doctor?.Name ?? "N/A",
                "N/A", 
                a.Status
            ));
        }


        public async Task<IEnumerable<AppointmentModel.SearchResponse>> GetAppointmentsByDate(DateTime date)
        {
            // Búsqueda de turnos del día (GET /api/appointments?date=YYYY-MM-DD)
            var appointments = await _persistence.GetFiltered<Appointment>(
                a => a.AvailabilitySlot != null && a.AvailabilitySlot.SlotDate == date.Date,
                "AvailabilitySlot", "AvailabilitySlot.AvailabilityRule.Doctor.Speciality"
            );

            if (appointments == null) return new List<AppointmentModel.SearchResponse>();

            return appointments.Select(a => new AppointmentModel.SearchResponse(
                a.AvailabilitySlot?.AvailabilityRule?.Doctor?.Speciality?.Name ?? "N/A",
                a.AvailabilitySlot?.AvailabilityRule?.Doctor?.Name ?? "N/A",
                a.AvailabilitySlot.SlotDate,
                a.AvailabilitySlot.StartTime
            ));
        }

        public async Task<Pagination<AppointmentModel.SearchResponse>> SearchAppointments(Guid? specialtyId, Guid? doctorId, long? dni, DateTime? date, int pageSize, int pageIndex)
        {
         
            var dniStr = dni?.ToString();

            var pagedData = await _persistence.Paginate<Appointment, DateTime>(
                pageSize,
                pageIndex,
                a => (!doctorId.HasValue || a.AvailabilitySlot.AvailabilityRule.DoctorId == doctorId.Value) &&
                     (!specialtyId.HasValue || a.AvailabilitySlot.AvailabilityRule.Doctor.SpecialityId == specialtyId.Value) &&
                     (string.IsNullOrEmpty(dniStr) || a.Patient.Dni == dniStr) &&
                     (!date.HasValue || a.AvailabilitySlot.SlotDate == date.Value.Date),
                a => a.AvailabilitySlot.SlotDate, 
                "AvailabilitySlot", "AvailabilitySlot.AvailabilityRule.Doctor", "AvailabilitySlot.AvailabilityRule.Doctor.Speciality", "Patient"
            );

            return pagedData.Map(a => new AppointmentModel.SearchResponse(
                a.AvailabilitySlot?.AvailabilityRule?.Doctor?.Speciality?.Name ?? "N/A",
                a.AvailabilitySlot?.AvailabilityRule?.Doctor?.Name ?? "N/A",
                a.AvailabilitySlot.SlotDate,
                a.AvailabilitySlot.StartTime
            ));
        }
    }
}