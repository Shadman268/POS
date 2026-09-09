using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Medicine> Medicines { get; set; } = null!;
        public DbSet<TenantMedicine> TenantMedicines { get; set; } = null!;
        public DbSet<MedicineStock> MedicineStocks { get; set; } = null!;
        public DbSet<MedicineBatch> MedicineBatches { get; set; } = null!;
        public DbSet<StockMovement> StockMovements { get; set; } = null!;
        public DbSet<Receipt> Receipts { get; set; } = null!;
        public DbSet<ReceiptItem> ReceiptItems { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Tenant> Tenants { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Receipt>().HasKey(r => r.Id);
            modelBuilder.Entity<ReceiptItem>().HasKey(ri => ri.Id);

            modelBuilder.Entity<ReceiptItem>()
                .HasOne(ri => ri.Receipt)
                .WithMany(r => r.Items)
                .HasForeignKey(ri => ri.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReceiptItem>()
                .HasOne(ri => ri.Medicine)
                .WithMany()
                .HasForeignKey(ri => ri.MedicineId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReceiptItem>()
                .HasOne(ri => ri.TenantMedicine)
                .WithMany()
                .HasForeignKey(ri => ri.TenantMedicineId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReceiptItem>()
                .HasOne(ri => ri.MedicineBatch)
                .WithMany()
                .HasForeignKey(ri => ri.MedicineBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.TenantId, u.Username })
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TenantMedicine>()
                .HasIndex(tm => new { tm.TenantId, tm.MedicineId })
                .IsUnique();

            modelBuilder.Entity<TenantMedicine>()
                .HasOne(tm => tm.Tenant)
                .WithMany(t => t.TenantMedicines)
                .HasForeignKey(tm => tm.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TenantMedicine>()
                .HasOne(tm => tm.Medicine)
                .WithMany(m => m.TenantMedicines)
                .HasForeignKey(tm => tm.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicineStock>()
                .HasIndex(ms => ms.TenantMedicineId)
                .IsUnique();

            modelBuilder.Entity<MedicineStock>()
                .HasOne(ms => ms.TenantMedicine)
                .WithOne(tm => tm.Stock)
                .HasForeignKey<MedicineStock>(ms => ms.TenantMedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicineBatch>()
                .HasIndex(mb => new { mb.TenantMedicineId, mb.BatchNumber })
                .IsUnique();

            modelBuilder.Entity<MedicineBatch>()
                .HasIndex(mb => new { mb.TenantId, mb.ExpiryDate });

            modelBuilder.Entity<Medicine>()
                .HasIndex(m => m.Name);

            modelBuilder.Entity<Medicine>()
                .HasIndex(m => m.GenericName);

            modelBuilder.Entity<Receipt>()
                .HasOne(r => r.Tenant)
                .WithMany(t => t.Receipts)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Receipt>()
                .HasOne(r => r.OriginalReceipt)
                .WithMany()
                .HasForeignKey(r => r.OriginalReceiptId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Receipt>()
                .HasIndex(r => new { r.TenantId, r.OriginalReceiptId });

            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.ShopCode)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => new { sm.TenantId, sm.CreatedAtUtc });
        }
    }
}
