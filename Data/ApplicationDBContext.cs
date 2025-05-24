using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Evacuation_Planning_and_Monitoring_API.Data
{
    public class ApplicationDBContext : DbContext
    {
        //public ApplicationDBContext()
        //{
        //}
        //public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options, string connectionString) : base(options)
        //{
        //    // Use the connection string if provided
        //    if (!string.IsNullOrEmpty(connectionString))
        //    {
        //        Database.GetDbConnection().ConnectionString = connectionString;
        //    }
        //}

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<EvacuationZone> EvacuationZones { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            // Configure the EvacuationZone entity
            builder.Entity<EvacuationZone>(entity =>
            {
                entity.ToTable("EvacuationZones");
                entity.HasKey(e => e.ZoneID);
                entity.Property(e => e.ZoneID).IsRequired().HasMaxLength(10);
                entity.Property(e => e.NumberOfPeople).IsRequired();
                entity.Property(e => e.UrgencyLevel).IsRequired().HasDefaultValue(1);
                entity.OwnsOne(e => e.LocationCoordinates, loc =>
                {
                    loc.Property(l => l.Latitude).IsRequired();
                    loc.Property(l => l.Longitude).IsRequired();
                });

            });

            // Configure the Vehicle entity
            builder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicles"); // Specify the table name
                entity.HasKey(v => v.VehicleID);
                entity.Property(v => v.VehicleID).IsRequired().HasMaxLength(10);
                entity.Property(v => v.Capacity).IsRequired();
                entity.Property(v => v.Type).IsRequired().HasMaxLength(50);
                entity.Property(v => v.Speed).IsRequired();
                entity.OwnsOne(v => v.LocationCoordinates, loc =>
                {
                    loc.Property(l => l.Latitude).IsRequired();
                    loc.Property(l => l.Longitude).IsRequired();
                });
            });






        }
    }
}
