using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("PATIENTS");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Dni)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(p => p.Dni)
                .IsUnique();

            builder.Property(p => p.FullName)
                .HasMaxLength(150);

        }
    }
}
