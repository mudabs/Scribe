using Microsoft.EntityFrameworkCore;
using Scribe.Models;

namespace Scribe.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Brand>? Brands { get; set; }
        public DbSet<Model>? Models { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<Location>? Locations { get; set; }
        public DbSet<SerialNumber>? SerialNumbers { get; set; }
        public DbSet<Maintenance>? Maintenances { get; set; }
        public DbSet<Condition>? Condition { get; set; }
        public DbSet<Department>? Department { get; set; }
        public DbSet<SystemUser> SystemUsers { get; set; } = default!;
        public DbSet<Log> Log { get; set; } = default!;
        public DbSet<Group> Group { get; set; } = default!;
        public DbSet<UserGroup> UserGroup { get; set; } = default!;
        public DbSet<SerialNumberGroup> SerialNumberGroup { get; set; } = default!;
        public DbSet<ADUsers> ADUsers { get; set; } = default!;
        public DbSet<ServiceLog> ServiceLogs { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data for Condition
            modelBuilder.Entity<Condition>().HasData(
                new Condition
                {
                    Id = 1,
                    Name = "New",
                    ColorCode = "green"
                },
                new Condition
                {
                    Id = 2,
                    Name = "Needs Repairs",
                    ColorCode = "yellow"
                },
                new Condition
                {
                    Id = 3,
                    Name = "In Use",
                    ColorCode = "blue"
                },
                new Condition
                {
                    Id = 4,
                    Name = "Out Of Order",
                    ColorCode = "red"
                },
                new Condition
                {
                    Id = 5,
                    Name = "Awaiting User",
                    ColorCode = "grey"
                }
            );

            // Seed data for Location
            modelBuilder.Entity<Location>().HasData(
                new Location
                {
                    Id = 1,
                    Name = "New Storage",
                    Description = "IT Storage room"
                }
            );

            // Seed data for Department
            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = 1,
                    Name = "IT"
                },
                new Department
                {
                    Id = 2,
                    Name = "No Department"
                }
            );

            // Seed data for ADUsers
            modelBuilder.Entity<ADUsers>().HasData(
                new ADUsers
                {
                    Id = 1,
                    Name = "No User"
                }
            );

            modelBuilder.Entity<SerialNumber>()
                .HasOne(s => s.ADUsers)
                .WithMany(u => u.SerialNumbers)
                .HasForeignKey(s => s.ADUsersId)
                .OnDelete(DeleteBehavior.SetNull); // Prevent cascading deletion
        }

        public DbSet<IndividualAssignment> IndividualAssignment { get; set; } = default!;
        public DbSet<ServiceHistory> ServiceHistory { get; set; } = default!;
        public DbSet<AllocationHistory> AllocationHistory { get; set; } = default!;

    }
}