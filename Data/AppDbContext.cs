using KodaStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product", "dbo", table =>
            {
                table.HasCheckConstraint("CK_Product_Price", "[Price] >= 0");
                table.HasCheckConstraint("CK_Product_Stock", "[Stock] >= 0");
            });

            entity.HasKey(p => p.ProductCode);
            entity.Property(p => p.ProductCode).HasMaxLength(30).IsRequired().ValueGeneratedNever();
            entity.Property(p => p.Name).HasMaxLength(120).IsRequired();
            entity.Property(p => p.Price).IsRequired();
            entity.Property(p => p.Stock).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(30).IsRequired();

            entity.HasOne(p => p.Customer).WithMany(c => c.Products).HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer", "dbo");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).HasMaxLength(120).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Phone).HasMaxLength(15).IsRequired();
            entity.Property(c => c.Address).HasMaxLength(255).IsRequired();

            entity.Navigation(c => c.Products).HasField("_products").UsePropertyAccessMode(PropertyAccessMode.Field);

        });
    }
}