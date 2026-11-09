using KodaStore.Api.Data;
using KodaStore.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Api.Repositories;

public class EfProductRepository(AppDbContext db) : IProductRepository
{
    private readonly AppDbContext _db = db;

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _db.Products.AsNoTracking().OrderBy(p => p.ProductCode).ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByCodeAsync(string productCode, CancellationToken cancellationToken)
    {
        return await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductCode == productCode, cancellationToken);
    }

    public async Task<bool> TryAddAsync(Product product, CancellationToken cancellationToken)
    {
        _db.Products.Add(product);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 })
        {

            return false;
        }
    }

    public async Task<bool> TryDeleteAsync(string productCode, CancellationToken cancellationToken)
    {
        var code = productCode.Trim().ToUpperInvariant();

        var existing = await _db.Products.FirstOrDefaultAsync(p => p.ProductCode == productCode, cancellationToken);

        if (existing is null)
        {
            return false;
        }
        _db.Products.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TryUpdateAsync(Product product, CancellationToken cancellationToken)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.ProductCode == product.ProductCode, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        existing.UpdateDetail(
            product.CustomerId,
            product.ProductCode,
            product.Name,
            product.Price,
            product.Stock,
            product.Category
        );

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}