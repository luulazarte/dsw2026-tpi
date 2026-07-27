using Dsw2026Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentModel
    {
        public record Request(Guid AvailabilitySlotId, Guid PatientId, string Reason);
        public record Response(Guid Id, Guid AvailabilitySlotId, Guid PatientId, string Reason, AppointmentStatus Status);
    }
}
