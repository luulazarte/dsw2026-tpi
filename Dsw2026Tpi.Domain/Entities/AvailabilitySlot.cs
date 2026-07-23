using Dsw2026Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class AvailabilitySlot : EntityBase
    {
        public Guid AvailabilityRuleId { get; private set; }
        public AvailabilityRule AvailabilityRule { get; private set; } 
        public DateTime SlotDate { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public SlotStatus Status { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private AvailabilitySlot() { }
#pragma warning restore CS8618
        #endregion
        public AvailabilitySlot(Guid availabilityRuleId, DateTime slotDate, TimeSpan startTime, TimeSpan endTime, SlotStatus status = SlotStatus.AVAILABLE, Guid? id = null) : base(id)
        {
            AvailabilityRuleId = availabilityRuleId;
            SlotDate = slotDate.Date;
            StartTime = startTime;
            EndTime = endTime;
            Status = status;
        }
        public void Book()
        {
            Status = SlotStatus.BOOKED;
            UpdatedAt = DateTime.Now;
        }
        public void Release()
        {
            Status = SlotStatus.AVAILABLE;
            UpdatedAt = DateTime.Now;
        }
    }
}

