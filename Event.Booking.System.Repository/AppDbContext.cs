using Event.Booking.System.Core.Enums;
using Event.Booking.System.Core.Models;

using Microsoft.EntityFrameworkCore;

using System.Data;

namespace Event.Booking.System.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {

        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Core.Models.Event> Events { get; set; } = null!;
        public DbSet<TicketType> TicketTypes { get; set; } = null!;
        public DbSet<Core.Models.Booking> Bookings { get; set; } = null!;
        public DbSet<WaitingListEntry> WaitingListEntries { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
            // Seed initial data for Roles, Users, Clients, and UserRoles.
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = new Guid("a5a65e94-3d4a-4f9a-9b6c-67a4b3e5fa91"),
                    FullName = "Sola Akinfosile",
                    Email = "admin@hotmail.com",
                    Roles = RoleTypeEnum.Admin,
                    PhoneNumber = "+2348034336608",
                    PasswordHash = "ttt",
                    CreatedBy = new Guid("a5a65e94-3d4a-4f9a-9b6c-67a4b3e5fa91"),
                    CreatedDate = new DateTime(2025, 10, 17),
                }

            );

            modelBuilder.Entity<TicketType>()
           .HasOne(t => t.Event)
           .WithMany(e => e.TicketTypes)
           .HasForeignKey(t => t.EventId)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Core.Models.Event>()
                .HasMany(e => e.TicketTypes)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Core.Models.Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<WaitingListEntry>()
                .HasIndex(w => new { w.UserId, w.EventId })
                .IsUnique(); // prevent duplicate waiting list entries

        }
    }
}
