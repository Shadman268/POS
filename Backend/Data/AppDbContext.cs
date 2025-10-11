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
        }
    }
}
