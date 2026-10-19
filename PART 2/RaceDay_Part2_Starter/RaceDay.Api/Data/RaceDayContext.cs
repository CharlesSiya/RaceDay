using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayContext(DbContextOptions<RaceDayContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(256);
        modelBuilder.Entity<Category>().Property(c => c.DistanceKm).HasPrecision(8, 2);
        modelBuilder.Entity<Category>().Property(c => c.EntryFee).HasPrecision(10, 2);
        modelBuilder.Entity<Route>().Property(r => r.DistanceKm).HasPrecision(8, 2);
        modelBuilder.Entity<Result>().Property(r => r.PaceMinutesPerKm).HasPrecision(8, 2);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser).WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event).WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Route>()
            .HasOne(r => r.Event).WithMany(e => e.Routes)
            .HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasOne(en => en.Participant).WithMany(u => u.Enrollments)
            .HasForeignKey(en => en.ParticipantId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(en => en.Category).WithMany(c => c.Enrollments)
            .HasForeignKey(en => en.CategoryId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrollment).WithOne(en => en.Result)
            .HasForeignKey<Result>(r => r.EnrollmentId).OnDelete(DeleteBehavior.Cascade);
    }
}
