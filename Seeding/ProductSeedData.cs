using System.Diagnostics.Tracing;
using KodaStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Api.Seeding;

public static class ProductSeedData
{
    private static IEnumerable<Product> InitialProduct()
    {
        // yield return new Product(1, "PR004", "Keyboard Mekanik", 135000L, 20, "Elektronik");
        // yield return new Product(1, "PR005", "Mouse", 20000L, 20, "Mouse");
        // yield return new Product(1, "PR006", "Monitor", 1400000L, 20, "Screen");

        // 10 Data
        for (int i = 1; i <= 10; i++)
        {
            string productCode = $"PR{i:D3}";
            string productName = $"Product Dummy {i}";

            long price = 15000L + (i * 1000L);
            int stock = 20;

            int customerId = 1;

            string category = i % 3 == 0 ? "Elektronik" : (i % 2 == 0) ? "Mouse" : "Screen";

            yield return new Product(customerId, productCode, productName, price, stock, category);
        }
    }

    public static void Seed(DbContext context)
    {
        foreach (var product in InitialProduct())
        {
            bool exists = context.Set<Product>().Any(p => p.ProductCode == product.ProductCode);

            if (!exists)
            {
                context.Set<Product>().Add(product);
            }
        }

        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        foreach (var product in InitialProduct())
        {
            bool exists = context.Set<Product>().Any(p => p.ProductCode == product.ProductCode);

            if (!exists)
            {
                context.Set<Product>().Add(product);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}