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
                entity.OwnsOne(e => e.LocationCoordinates, loc =>
                {
                    loc.Property(l => l.Latitude);
                    loc.Property(l => l.Longitude);
                });
            });

            // Configure the Vehicle entity
            builder.Entity<Vehicle>(entity =>
            {
                entity.OwnsOne(v => v.LocationCoordinates, loc =>
                {
                    loc.Property(l => l.Latitude);
                    loc.Property(l => l.Longitude);
                });
            });







        }
    }
}
