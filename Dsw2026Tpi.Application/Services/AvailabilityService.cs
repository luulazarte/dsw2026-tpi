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
            return await GenerarDisponibilidades(request);
        }

        // ---> ACÁ ESTÁ EL CAMBIO: Ahora devuelve la lista de DTOs <---
        public async Task<List<AvailabilityModel.Response>> Update(AvailabilityModel.Request request)
        {
            return await GenerarDisponibilidades(request);
        }

        // ---> ACÁ ESTÁ EL CAMBIO: El método privado retorna List<AvailabilityModel.Response> <---
        private async Task<List<AvailabilityModel.Response>> GenerarDisponibilidades(AvailabilityModel.Request request)
        {
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null || doctor.Deleted)
            {
                throw new Exception("El doctor especificado no existe.");
            }

            var hoy = DateTime.Today;
            var ultimoDiaMes = new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month));

            // ---> ACÁ ESTÁ EL CAMBIO: Inicializamos la lista que va a contener las respuestas <---
            var response = new List<AvailabilityModel.Response>();

            foreach (var daySchedule in request.Days)
            {
                if (!TimeSpan.TryParse(daySchedule.StartTime, out TimeSpan startTime) ||
                    !TimeSpan.TryParse(daySchedule.EndTime, out TimeSpan endTime))
                {
                    throw new Exception($"Formato de hora inválido para el día {daySchedule.Day}.");
                }

                if (startTime >= endTime)
                {
                    throw new Exception($"La hora de inicio debe ser menor a la de fin para el día {daySchedule.Day}.");
                }

                if (!_diasSemana.TryGetValue(daySchedule.Day, out DayOfWeek diaBuscado))
                {
                    throw new Exception($"Día no válido: {daySchedule.Day}");
                }

                var rule = new AvailabilityRule(
                    request.DoctorId,
                    hoy.Month,
                    (short)hoy.Year,
                    (short)diaBuscado,
                    startTime,
                    endTime
                );

                await _persistence.Add(rule);

                DateTime fechaIteracion = hoy;
                while (fechaIteracion <= ultimoDiaMes)
                {
                    if (fechaIteracion.DayOfWeek == diaBuscado)
                    {
                        TimeSpan horaActual = startTime;
                        while (horaActual < endTime)
                        {
                            TimeSpan horaFinSlot = horaActual.Add(TimeSpan.FromMinutes(30));
                            if (horaFinSlot > endTime) break;

                            var slot = new AvailabilitySlot(
                                rule.Id,
                                fechaIteracion,
                                horaActual,
                                horaFinSlot,
                                SlotStatus.AVAILABLE
                            );

                            await _persistence.Add(slot);

                            // ---> ACÁ ESTÁ EL CAMBIO: Agregamos el slot mapeado a la lista de respuesta usando tu record <---
                            response.Add(new AvailabilityModel.Response(
                                slot.Id,
                                rule.Id,
                                slot.SlotDate,
                                slot.StartTime,
                                slot.EndTime,
                                slot.Status
                            ));

                            horaActual = horaFinSlot;
                        }
                    }
                    fechaIteracion = fechaIteracion.AddDays(1);
                }
            }

            // ---> ACÁ ESTÁ EL CAMBIO: Retornamos la lista con todos los slots generados <---
            return response;
        }
    }
}