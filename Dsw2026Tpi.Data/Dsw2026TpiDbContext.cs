using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Dsw2026Tpi.Data;

    public class Dsw2026TpiDbContext : DbContext
    {
        public Dsw2026TpiDbContext(DbContextOptions<Dsw2026TpiDbContext> options): base(options)
        {
        }

        public DbSet<Speciality> Specialities => Set<Speciality>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
        public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        
     public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.Now;
                entry.Entity.UpdatedAt = DateTime.Now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.Now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }


}
