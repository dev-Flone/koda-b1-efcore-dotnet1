using System.Data;
using KodaStore.Api.Models;
using Microsoft.Data.SqlClient;

namespace KodaStore.Api.Repositories;

public class SqlProductRepository(IConfiguration configuration) : IProductRepository
{
    private readonly string _connetionString = configuration.GetConnectionString("ecommercedb") ?? throw new InvalidOperationException("Connection string ecommerce menghilang");

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """"
            SELECT CustomerId, ProductCode, Name, Price, Stock, Category
            FROM dbo.Product
            ORDER BY ProductCode DESC;
        """";

        var products = new List<Product>();

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(ReadProduct(reader));
        }
        return products;
    }

    public async Task<Product?> GetByCodeAsync(string productCode, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT CustomerId, ProductCode, Name, Price, Stock, Category
            FROM dbo.Product
            WHERE ProductCode = @productCode;
        """;

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddProductCode(command, productCode);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }
        return ReadProduct(reader);

    }

    public async Task<bool> TryAddAsync(Product product, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Product(CustomerId, ProductCode, Name, Price, Stock, Category)
            VALUES (@customerId, @productCode, @name, @price, @stock, @category);
        """;
        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddProductParameter(command, product);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            return affectedRows == 1;
        }
        catch (SqlException e) when (e.Number is 2601 or 2627)
        {
            return false;
        }
    }

    public async Task<bool> TryDeleteAsync(string productCode, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM dbo.Product 
            WHERE ProductCode = @productCode;
        """;

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddProductCode(command, productCode);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        return affectedRows == 1;

    }

    public async Task<bool> TryUpdateAsync(Product product, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Product
            SET 
                CustomerId = @customerId,
                ProductCode = @productCode,
                Name = @name,
                Price = @price,
                Stock = @stock,
                Category = @category
            WHERE ProductCode = @productCode;
        """;

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddProductParameter(command, product);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        return affectedRows == 1;
    }

    // METHOD
    private static void AddProductCode(SqlCommand command, string productCode)
    {
        command.Parameters.Add("@productCode", SqlDbType.NVarChar, 30).Value = productCode.Trim().ToUpperInvariant();
    }

    private static void AddProductParameter(SqlCommand command, Product product)
    {
        command.Parameters.Add("@customerId", SqlDbType.Int).Value = product.CustomerId;

        AddProductCode(command, product.ProductCode);

        command.Parameters.Add("@name", SqlDbType.NVarChar, 120).Value = product.Name;

        command.Parameters.Add("@stock", SqlDbType.Int).Value = product.Stock;

        var price = command.Parameters.Add("@price", SqlDbType.BigInt);

        command.Parameters.Add("@category", SqlDbType.Text, 80).Value = product.Category;

        price.Precision = 18;
        price.Scale = 2;
        price.Value = product.Price;
    }

    private static Product ReadProduct(SqlDataReader reader)
    {
        return new Product(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt64(3),
            reader.GetInt32(4),
            reader.GetString(5)
        );
    }


}