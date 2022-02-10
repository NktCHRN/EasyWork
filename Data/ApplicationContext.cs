using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class ApplicationContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<Ban> Bans => Set<Ban>();

        public DbSet<Entities.File> Files => Set<Entities.File>();

        public DbSet<Message> Messages => Set<Message>();

        public DbSet<Project> Projects => Set<Project>();

        public DbSet<Release> Releases => Set<Release>();

        public DbSet<Entities.Task> Tasks => Set<Entities.Task>();

        public DbSet<UserOnProject> UsersOnProjects => Set<UserOnProject>();

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserOnProject>()
            .HasKey(u => new { u.UserId, u.ProjectId });

            builder.Entity<Entities.File>()
            .HasOne(f => f.Message)
            .WithMany(m => m.Files)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Entities.File>()
            .HasOne(f => f.Task)
            .WithMany(t => t.Files)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
            .HasOne(m => m.Task)
            .WithMany(t => t.Messages)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Entities.Task>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Release>()
            .HasOne(r => r.Project)
            .WithMany(p => p.Releases)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserOnProject>()
            .HasOne(u => u.Project)
            .WithMany(p => p.TeamMembers)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasIndex(u => u.PhoneNumber).IsUnique();
        }
    }
}
