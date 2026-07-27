using Dsw2026Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AvailabilityModel
    {
        public record Request(Guid AvailabilityRuleId, DateTime SlotDate, TimeSpan StartTime, TimeSpan EndTime);
        public record Response(Guid Id, Guid AvailabilityRuleId, DateTime SlotDate, TimeSpan StartTime, TimeSpan EndTime, SlotStatus Status);
    }
}
