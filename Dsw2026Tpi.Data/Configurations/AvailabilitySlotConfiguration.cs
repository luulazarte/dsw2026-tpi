using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
    {
        public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
        {
            builder.ToTable("AVAILABILITYSLOT");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SlotDate).IsRequired();
            builder.Property(s => s.StartTime).IsRequired();
            builder.Property(s => s.EndTime).IsRequired();

            builder.Property(s => s.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);


            builder.HasOne(s => s.AvailabilityRule)
                .WithMany()
                .HasForeignKey(s => s.AvailabilityRuleId);
                

        }
    }
}
