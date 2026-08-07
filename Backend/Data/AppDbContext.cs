using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Receipt> Receipts { get; set; } = null!;
        public DbSet<ReceiptItem> ReceiptItems { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Tenant> Tenants { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Receipt primary key
            modelBuilder.Entity<Receipt>()
                .HasKey(r => r.Id);

            // Configure ReceiptItem primary key
            modelBuilder.Entity<ReceiptItem>()
                .HasKey(ri => ri.Id);

            // Configure Receipt-ReceiptItem relationship
            modelBuilder.Entity<ReceiptItem>()
                .HasOne(ri => ri.Receipt)
                .WithMany(r => r.Items)
                .HasForeignKey(ri => ri.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ReceiptItem-Product relationship
            modelBuilder.Entity<ReceiptItem>()
                .HasOne(ri => ri.Product)
                .WithMany()
                .HasForeignKey(ri => ri.ProductId);

            // Configure User
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.TenantId, u.Username })
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Tenant)
                .WithMany(t => t.Products)
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Receipt>()
                .HasOne(r => r.Tenant)
                .WithMany(t => t.Receipts)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.ShopCode)
                .IsUnique();

            // Configure RefreshToken
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
        }
    }
}
