﻿using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Evacuation_Planning_and_Monitoring_API.Data
{
    public class ApplicationDBContext : DbContext
    {

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<EvacuationZone> EvacuationZones { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<EvacuationPlan> EvacuationPlans { get; set; }
        public DbSet<EvacuationStatus> EvacuationStatuses { get; set; }

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

            // กำหนดให้ ZoneID ใน EvacuationPlan เป็น Foreign Key
            builder.Entity<EvacuationPlan>()
                .HasOne<EvacuationZone>()
                .WithMany()
                .HasForeignKey(p => p.ZoneID);

            // กำหนดให้ VehicleID ใน EvacuationPlan เป็น Foreign Key
            builder.Entity<EvacuationPlan>()
                .HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey(p => p.VehicleID);







        }
    }
}
