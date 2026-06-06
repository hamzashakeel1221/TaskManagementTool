using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Models;

namespace TaskManagement.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TaskItem>(entity =>
            {
                entity.HasOne(t => t.Owner)
                      .WithMany(u => u.CreatedTasks)
                      .HasForeignKey(t => t.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.AssignedTo)
                      .WithMany(u => u.AssignedTasks)
                      .HasForeignKey(t => t.AssignedToId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(t => t.Status)
                      .HasConversion<string>();

                entity.Property(t => t.Priority)
                      .HasConversion<string>();
            });

            // Seed categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Development" },
                new Category { Id = 2, Name = "Design" },
                new Category { Id = 3, Name = "Testing" },
                new Category { Id = 4, Name = "DevOps" },
                new Category { Id = 5, Name = "Management" }
            );
        }
    }
}
