using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class AvailabilityRule : EntityBase
    {
        public Guid DoctorId { get; private set; }
        public Doctor Doctor { get; private set; } 
        public int Month { get; private set; }
        public int Year { get; private set; }
        public string DayOfWeek { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public ICollection<AvailabilitySlot> Slots { get; private set; } = new List<AvailabilitySlot>();

        #region Constructor for EF
#pragma warning disable CS8618
        private AvailabilityRule() { }
#pragma warning restore CS8618
        #endregion

        public AvailabilityRule(Guid doctorId, int month, int year, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, Guid? id = null) : base(id)
        {
            DoctorId = doctorId;
            Month = month;
            Year = year;
            DayOfWeek = dayOfWeek.ToUpper();
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}   
