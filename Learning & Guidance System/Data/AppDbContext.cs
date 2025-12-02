using Learning_Guidance_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Learning_Guidance_System.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<SemesterResult> SemesterResults => Set<SemesterResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasMany(s => s.SemesterResults)
                .WithOne(r => r.Student)
                .HasForeignKey(r => r.StudentId);

            // One result per semester per student
            modelBuilder.Entity<SemesterResult>()
                .HasIndex(r => new { r.StudentId, r.SemesterNumber })
                .IsUnique();
        }
    }
}
