using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
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

        public async Task<AppointmentModel.PatientResponse> Create(AppointmentModel.Request request)
        {

            var dniStr = request.Patient.Dni.ToString();
            if (dniStr.Length < 7 || dniStr.Length > 10)
                throw new ValidationException("El DNI es obligatorio y debe tener entre 7 y 10 dígitos.", nameof(ErrorCodes.VALIDATION_ERROR));

            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
                throw new ValidationException("El motivo debe tener al menos 5 caracteres.", nameof(ErrorCodes.VALIDATION_ERROR));

            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null || doctor.Deleted)
                throw new ValidationException("El doctor especificado no existe.", nameof(ErrorCodes.VALIDATION_ERROR));

            var patient = await _persistence.First<Patient>(p => p.Dni == dniStr);
            if (patient == null)
                throw new ValidationException("No existe un paciente registrado con ese DNI.", nameof(ErrorCodes.VALIDATION_ERROR));

            var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilitySlotId);
            if (slot == null)
                throw new ValidationException("El slot no existe.", nameof(ErrorCodes.VALIDATION_ERROR));

            if (slot.SlotDate < DateTime.Today || (slot.SlotDate == DateTime.Today && slot.StartTime < DateTime.Now.TimeOfDay))
                throw new ValidationException("No se permiten turnos en el pasado.", nameof(ErrorCodes.VALIDATION_ERROR));

            if (slot.Status != SlotStatus.AVAILABLE)
                throw new ConflictException("APPOINTMENT_CONFLICT", "Slot already booked");

            var appointment = new Appointment(slot.Id, patient.Id, request.Reason);
            await _persistence.Add(appointment);


            slot.Book();
            await _persistence.Update(slot);


            return new AppointmentModel.PatientResponse(
                appointment.Id,
                slot.SlotDate,
                slot.StartTime,
                doctor.Name,
                doctor.Speciality?.Name ?? "N/A",
                appointment.Status
            
            );
        }

        public async Task Cancel(Guid id)
        {
            var appointment = await _persistence.GetById<Appointment>(id);
            if (appointment == null)
                throw new EntityNotFoundException("Turno");

            if (appointment.Status != AppointmentStatus.BOOKED)
                throw new ValidationException("Solo se puede cancelar un turno en estado BOOKED.", nameof(ErrorCodes.VALIDATION_ERROR));

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


        public async Task<Pagination<AppointmentModel.SearchResponse>> GetAppointmentsByDate(DateTime date, int pageSize, int pageIndex)
        {
            var pagedData = await _persistence.Paginate<Appointment, DateTime>(
                pageSize,
                pageIndex,
                a => a.AvailabilitySlot != null && a.AvailabilitySlot.SlotDate == date.Date,
                a => a.AvailabilitySlot.SlotDate,
                "AvailabilitySlot", "AvailabilitySlot.AvailabilityRule.Doctor.Speciality"
            );

            return pagedData.Map(a => new AppointmentModel.SearchResponse(
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