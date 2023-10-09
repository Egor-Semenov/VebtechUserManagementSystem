using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(x => new { x.Id });

            modelBuilder.Entity<UserRoles>()
                .HasKey(x => new { x.Role, x.UserId });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
    }
}
