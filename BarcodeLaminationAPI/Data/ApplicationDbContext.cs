using BarcodeLaminationModel.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BarcodeLaminationAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Material> Materials { get; set; } 
        public DbSet<FilmCoatingRecord> FilmCoatingRecords { get; set; }
        public DbSet<FeedingRecord> FeedingRecords { get; set; }
        public DbSet<UnloadingRecord> UnloadingRecords { get; set; }
        public DbSet<FeedingRecordsList> FeedingRecordsList { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Material配置
            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasIndex(m => m.ProductERPCode).IsUnique();
                entity.HasIndex(m => m.MoldNumber).IsUnique();
                entity.Property(m => m.FabricRollCount).HasDefaultValue(3);
                entity.Property(m => m.IsActive).HasDefaultValue(true);
                entity.Property(m => m.CreateTime).HasDefaultValueSql("GETDATE()");
            });

            // FilmCoatingRecord配置
            modelBuilder.Entity<FilmCoatingRecord>(entity =>
            {
                entity.HasIndex(f => f.NewERPCode).IsUnique();
                entity.HasIndex(f => f.PrintTime);
                entity.Property(f => f.PrintTime).HasDefaultValueSql("GETDATE()");
                entity.Property(f => f.CreatedTime).HasDefaultValueSql("GETDATE()");
            });

            // FeedingRecord配置
            modelBuilder.Entity<FeedingRecord>(entity =>
            {
                entity.HasIndex(f => f.FeedingTime);
                entity.HasIndex(f => f.ProductERPCode);
                entity.Property(f => f.FeedingTime).HasDefaultValueSql("GETDATE()");
                entity.Property(f => f.CreatedTime).HasDefaultValueSql("GETDATE()");
            });

            // UnloadingRecord配置
            modelBuilder.Entity<UnloadingRecord>(entity =>
            {
                entity.HasIndex(u => u.PrintTime);
                entity.HasIndex(u => u.ProductERPCode);
                entity.Property(u => u.PrintTime).HasDefaultValueSql("GETDATE()");
                entity.Property(u => u.CreatedTime).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}