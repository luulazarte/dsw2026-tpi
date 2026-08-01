using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Dsw2026Tpi.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IPersistence _persistence;

        private readonly Dictionary<string, DayOfWeek> _diasSemana = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Domingo", DayOfWeek.Sunday },
            { "Lunes", DayOfWeek.Monday },
            { "Martes", DayOfWeek.Tuesday },
            { "Miercoles", DayOfWeek.Wednesday },
            { "Miércoles", DayOfWeek.Wednesday },
            { "Jueves", DayOfWeek.Thursday },
            { "Viernes", DayOfWeek.Friday },
            { "Sabado", DayOfWeek.Saturday },
            { "Sábado", DayOfWeek.Saturday }
        };

        public AvailabilityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<List<AvailabilityModel.Response>> Create(AvailabilityModel.Request request)
        {
            var doctor = await ValidarDoctor(request.DoctorId);

            var hoy = DateTime.Today;
            var yaExiste = await _persistence.GetFiltered<AvailabilityRule>(
                r => r.DoctorId == doctor.Id && r.Month == hoy.Month && r.Year == (short)hoy.Year
            );

            if (yaExiste != null && yaExiste.Any())
                throw new ConflictException(nameof(ErrorCodes.VALIDATION_ERROR),
                    "El doctor ya tiene disponibilidad cargada para este mes. Usá PUT para modificarla.");

            return await GenerarDisponibilidades(request, doctor);
        }

        
        public async Task<List<AvailabilityModel.Response>> Update(AvailabilityModel.Request request)
        {
            var doctor = await ValidarDoctor(request.DoctorId);

            await BorrarDisponibilidadDelMes(doctor.Id);

            return await GenerarDisponibilidades(request, doctor);
        }

        

        private async Task<Doctor> ValidarDoctor(Guid doctorId)
        {
            var doctor = await _persistence.GetById<Doctor>(doctorId);
            if (doctor == null || doctor.Deleted)
                throw new ValidationException("El doctor especificado no existe.", nameof(ErrorCodes.VALIDATION_ERROR));
            return doctor;
        }

        private async Task BorrarDisponibilidadDelMes(Guid doctorId)
        {
            var hoy = DateTime.Today;

            var reglas = await _persistence.GetFiltered<AvailabilityRule>(
                r => r.DoctorId == doctorId && r.Month == hoy.Month && r.Year == (short)hoy.Year,
                "Slots"
            );

            if (reglas == null) return;

            foreach (var regla in reglas)
            {
                foreach (var slot in regla.Slots.ToList())
                    await _persistence.Delete(slot);

                await _persistence.Delete(regla);
            }
        }

        private async Task<List<AvailabilityModel.Response>> GenerarDisponibilidades(
            AvailabilityModel.Request request, Doctor doctor)
        {
            var hoy = DateTime.Today;
            var ultimoDiaMes = new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month));
            var feriados = CargarFeriados();
            var response = new List<AvailabilityModel.Response>();

            var horarios = new List<(DayOfWeek dia, TimeSpan inicio, TimeSpan fin)>();

            foreach (var daySchedule in request.Days)
            {
                if (!TimeSpan.TryParse(daySchedule.StartTime, out var startTime) ||
                    !TimeSpan.TryParse(daySchedule.EndTime, out var endTime))
                    throw new ValidationException($"Formato de hora inválido para el día {daySchedule.Day}.", nameof(ErrorCodes.VALIDATION_ERROR));

                if (startTime >= endTime)
                    throw new ValidationException($"La hora de inicio debe ser menor a la de fin para el día {daySchedule.Day}.", nameof(ErrorCodes.VALIDATION_ERROR));

                if (!_diasSemana.TryGetValue(daySchedule.Day, out var diaBuscado))
                    throw new ValidationException($"Día no válido: {daySchedule.Day}", nameof(ErrorCodes.VALIDATION_ERROR));

                horarios.Add((diaBuscado, startTime, endTime));
            }

            ValidarSolapamientos(horarios);

            foreach (var (diaBuscado, startTime, endTime) in horarios)
            {
                var rule = new AvailabilityRule(
                    doctor.Id,
                    hoy.Month,
                    (short)hoy.Year,
                    (int)diaBuscado,
                    startTime,
                    endTime
                );
                await _persistence.Add(rule);

                var fechaIteracion = hoy;
                while (fechaIteracion <= ultimoDiaMes)
                {
                    bool esFeriado = feriados.Contains(fechaIteracion.Date);

                    if (fechaIteracion.DayOfWeek == diaBuscado && !esFeriado)
                    {
                        var horaActual = startTime;
                        while (horaActual < endTime)
                        {
                            var horaFinSlot = horaActual.Add(TimeSpan.FromMinutes(30));
                            if (horaFinSlot > endTime) break;

                            var slot = new AvailabilitySlot(rule.Id, fechaIteracion, horaActual, horaFinSlot, SlotStatus.AVAILABLE);
                            await _persistence.Add(slot);

                            response.Add(new AvailabilityModel.Response(
                                slot.Id, rule.Id, slot.SlotDate, slot.StartTime, slot.EndTime, slot.Status));

                            horaActual = horaFinSlot;
                        }
                    }
                    fechaIteracion = fechaIteracion.AddDays(1);
                }
            }

            return response;
        }

        private static void ValidarSolapamientos(List<(DayOfWeek dia, TimeSpan inicio, TimeSpan fin)> horarios)
        {
            foreach (var grupo in horarios.GroupBy(h => h.dia))
            {
                var ordenados = grupo.OrderBy(h => h.inicio).ToList();
                for (int i = 0; i < ordenados.Count - 1; i++)
                {
                    if (ordenados[i].fin > ordenados[i + 1].inicio)
                        throw new ValidationException($"Hay horarios solapados para el día {grupo.Key}.", nameof(ErrorCodes.VALIDATION_ERROR));
                }
            }
        }

        private static HashSet<DateTime> CargarFeriados()
        {
            try
            {
                var ruta = Path.Combine(AppContext.BaseDirectory, "Sources", "feriados.json");
                if (!File.Exists(ruta)) return new HashSet<DateTime>();

                var json = File.ReadAllText(ruta);
                var fechas = JsonSerializer.Deserialize<List<DateTime>>(json);
                return fechas == null ? new HashSet<DateTime>() : fechas.Select(f => f.Date).ToHashSet();
            }
            catch
            {
                return new HashSet<DateTime>();
            }
        }
    }
}