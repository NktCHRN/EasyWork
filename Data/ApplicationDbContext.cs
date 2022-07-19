using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<Ban> Bans => Set<Ban>();

        public DbSet<Entities.File> Files => Set<Entities.File>();

        public DbSet<Message> Messages => Set<Message>();

        public DbSet<Project> Projects => Set<Project>();

        public DbSet<Entities.Task> Tasks => Set<Entities.Task>();

        public DbSet<UserOnProject> UsersOnProjects => Set<UserOnProject>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserOnProject>()
            .HasKey(u => new { u.UserId, u.ProjectId });

            builder.Entity<Entities.File>()
            .HasOne(f => f.Task)
            .WithMany(t => t.Files)
            .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()
            .HasOne(m => m.Task)
            .WithMany(t => t.Messages)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Entities.Task>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserOnProject>()
            .HasOne(u => u.Project)
            .WithMany(p => p.TeamMembers)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserOnProject>()
            .HasOne(u => u.User)
            .WithMany(p => p.Projects)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Ban>()
            .HasOne(u => u.User)
            .WithMany(p => p.Bans)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
            .HasOne(u => u.User)
            .WithMany(p => p.RefreshTokens)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
