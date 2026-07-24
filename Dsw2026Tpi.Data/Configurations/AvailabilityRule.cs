using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
    {
        public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
        {
            builder.ToTable("AVAILABILITYRULES");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Month).IsRequired();
            builder.Property(r => r.Year).IsRequired();
            builder.Property(r => r.DayOfWeek).IsRequired();
            builder.Property(r => r.StartTime).IsRequired();
            builder.Property(r => r.EndTime).IsRequired();

            builder.HasOne(r => r.Doctor)
                .WithMany()
                .HasForeignKey(r => r.DoctorId);
                

            builder.HasIndex(r => new { r.DoctorId, r.Year, r.Month, r.DayOfWeek, r.StartTime, r.EndTime })
                .IsUnique();

        }
    }
}
