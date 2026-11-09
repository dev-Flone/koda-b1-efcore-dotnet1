namespace KodaStore.Api.Models;

public sealed class Product
{
    public int CustomerId { get; private set; }
    public string ProductCode { get; private set; }
    public string Name { get; private set; }
    public long Price { get; private set; }
    public int Stock { get; private set; }
    public string Category { get; private set; }
    public Customer? Customer { get; private set; }

    public Product(int customerId, string productCode, string name, long price, int stock, string category)
    {
        if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId), "Customer ID must be positive.");

        if (string.IsNullOrWhiteSpace(productCode) || productCode.Trim().Length > 30) throw new ArgumentException("Product code is required.", nameof(productCode));

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 30) throw new ArgumentException("Name is required.", nameof(name));

        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        if (stock < 0) throw new ArgumentException("Stock cannot be negative.", nameof(stock));

        if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 30) throw new ArgumentException("Category is required.", nameof(category));


        CustomerId = customerId;
        ProductCode = productCode.Trim().ToUpperInvariant();
        Name = name.Trim();
        Price = price;
        Stock = stock;
        Category = category.Trim();
    }

    public void UpdateDetail(int customerId, string productCode, string name, long price, int stock, string category)
    {
        var validated = new Product(customerId, productCode, name, price, stock, category);

        CustomerId = validated.CustomerId;
        ProductCode = validated.ProductCode;
        Name = validated.Name;
        Price = validated.Price;
        Stock = validated.Stock;
        Category = validated.Category;
    }
}