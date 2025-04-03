using FormPlay.Models;
using Microsoft.EntityFrameworkCore;

namespace FormPlay.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TpsReport> TpsReports { get; set; }
        public DbSet<TpsReportField> TpsReportFields { get; set; }
        public DbSet<TpsReportAction> TpsReportActions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<TpsReport>()
                .HasOne(t => t.InitiatedBy)
                .WithMany()
                .HasForeignKey(t => t.InitiatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TpsReport>()
                .HasOne(t => t.PartnerUser)
                .WithMany()
                .HasForeignKey(t => t.PartnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TpsReportField>()
                .HasOne(f => f.TpsReport)
                .WithMany(t => t.Fields)
                .HasForeignKey(f => f.TpsReportId);

            modelBuilder.Entity<TpsReportAction>()
                .HasOne(a => a.TpsReport)
                .WithMany(t => t.Actions)
                .HasForeignKey(a => a.TpsReportId);

            modelBuilder.Entity<TpsReportAction>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);
        }
    }
}
