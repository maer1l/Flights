using System;
using System.Collections.Generic;
using Flights.Models;
using Microsoft.EntityFrameworkCore;

namespace Flights.Data;

public partial class FlightsContext : DbContext
{
    public FlightsContext()
    {
    }

    public FlightsContext(DbContextOptions<FlightsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Flight> Flights { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.FlightId).HasName("PK__flight__E37057652ED761F6");

            entity.ToTable("flight");

            entity.Property(e => e.FlightId).HasColumnName("flight_id");
            entity.Property(e => e.Direction)
                .HasMaxLength(50)
                .HasColumnName("direction");
            entity.Property(e => e.FlightNumber)
                .HasMaxLength(10)
                .HasColumnName("flight_number");
            entity.Property(e => e.GateNumber)
                .HasMaxLength(10)
                .HasColumnName("gate_number");
            entity.Property(e => e.PeopleCount).HasColumnName("people_count");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
