using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace DFC.Digital.Tools.Repository.Accounts
{
    public partial class DFCUserAccountsContext : DbContext
    {
        public DFCUserAccountsContext()
        {
        }

        public DFCUserAccountsContext(DbContextOptions<DFCUserAccountsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Accounts> Accounts { get; set; }

        public virtual DbSet<Audit> Audit { get; set; }

        public virtual DbSet<CircuitBreaker> CircuitBreaker { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("DefaultConectionString");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Accounts>(entity =>
            {
                entity.HasKey(e => e.Mail);

                entity.Property(e => e.Mail)
                    .HasColumnName("mail")
                    .HasMaxLength(200)
                    .ValueGeneratedNever();

                entity.Property(e => e.A1lifecycleStateUpin)
                    .HasColumnName("A1LifecycleStateUPIN")
                    .HasMaxLength(50);

                entity.Property(e => e.Createtimestamp)
                    .HasColumnName("createtimestamp")
                    .HasColumnType("date");

                entity.Property(e => e.Modifytimestamp)
                    .HasColumnName("modifytimestamp")
                    .HasColumnType("date");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.SfaProviderUserType)
                    .HasColumnName("sfaProviderUserType")
                    .HasMaxLength(100);

                entity.Property(e => e.Uid)
                    .HasColumnName("uid")
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<Audit>(entity =>
            {
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Notes)
                    .HasMaxLength(5000)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TimeStamp).IsRowVersion();
            });

            modelBuilder.Entity<CircuitBreaker>(entity =>
            {
                entity.Property(e => e.CircuitBreakerStatus).HasMaxLength(50);

                entity.Property(e => e.LastCircuitOpenDate).HasColumnType("datetime");
            });
        }
    }
}
