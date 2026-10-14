using Microsoft.EntityFrameworkCore;
using My_Study_App.Models;

namespace My_Study_App.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<StudyPlan> StudyPlans { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Flashcard>()
                .HasOne(f => f.StudyPlan)
                .WithMany(sp => sp.Flashcards)
                .HasForeignKey(f => f.StudyPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudySession>()
                .HasIndex(s => new { s.UserId, s.LoginTicks })
                .IsUnique();
        }
    }
}