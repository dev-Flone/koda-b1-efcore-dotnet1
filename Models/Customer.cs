namespace KodaStore.Api.Models;

public sealed class Customer
{
    private readonly List<Product> _products = [];
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public IReadOnlyList<Product> Products => _products.AsReadOnly();

    public Customer(string name, string email, string phone, string address)
        : this(0, name, email, phone, address, [])
    {
    }

    public Customer(int id, string name, string email, string phone, string address, List<Product>? products = null)
    {
        if (id < 0) throw new ArgumentOutOfRangeException(nameof(id), "Customer ID cannot be negative.");

        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");

        if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentException("Phone is required");

        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Address is required");


        Id = id;
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        if (products != null) _products = products;
    }
}