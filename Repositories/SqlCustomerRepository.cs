using System.Data;
using KodaStore.Api.Models;
using Microsoft.Data.SqlClient;

namespace KodaStore.Api.Repositories;

public class SqlCustomerRepository(IConfiguration configuration) : ICustomerRepository
{
    private readonly string _connetionString = configuration.GetConnectionString("ecommercedb") ?? throw new InvalidOperationException("Connection string ecommerce menghilang");

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, Email, Phone, Address
            FROM Customer
            ORDER BY Id;
        """;

        var customers = new List<Customer>();

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            customers.Add(ReadCustomer(reader));
        }
        return customers;
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                c.Address,
                p.ProductCode,
                p.Name
            FROM dbo.Customer c
            LEFT JOIN dbo.Product p ON p.CustomerId = c.Id
            WHERE c.Id = @id
            ORDER BY p.ProductId;
        """;

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        Customer? customer = null;
        var products = new List<CustomerProduct>();

        while (await reader.ReadAsync(cancellationToken))
        {
            customer ??= new Customer(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4));

            if (!reader.IsDBNull(5))
            {
                products.Add(new CustomerProduct(
                    reader.GetString(5),
                    reader.GetString(6)));
            }
        }

        return customer is null
            ? null
            : new Customer(customer.Id, customer.Name, customer.Email, customer.Phone, customer.Address);
    }

    public async Task<Customer?> TryAddAsync(Customer customer, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO Customer(Name, Email, Phone, Address)
            VALUES (@name, @email, @phone, @address);
            SELECT CAST(SCOPE_IDENTITY() AS int);
        """;

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddCustomerParameter(command, customer);

        try
        {
            var id = (int?)await command.ExecuteScalarAsync(cancellationToken);
            return id is null
                ? null
                : new Customer(id.Value, customer.Name, customer.Email, customer.Phone, customer.Address);
        }
        catch (SqlException e) when (e.Number is 2601 or 2627)
        {
            return null;
        }
    }

    public async Task<bool> TryDeleteAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM Customer
            WHERE Id = @id;
        """;

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            return affectedRows == 1;
        }
        catch (SqlException e) when (e.Number == 547)
        {
            throw new InvalidOperationException("Cannot delete a customer that still has products.", e);
        }
    }

    public async Task<bool> TryUpdateAsync(int id, Customer customer, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE Customer
            SET
                Name = @name,
                Email = @email,
                Phone = @phone,
                Address = @address
            WHERE Id = @id;
        """;

        await using var connection = new SqlConnection(_connetionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddCustomerParameter(command, customer);
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows == 1;
    }

    private static void AddCustomerParameter(SqlCommand command, Customer customer)
    {
        command.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = customer.Name;
        command.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = customer.Email;
        command.Parameters.Add("@phone", SqlDbType.VarChar, 15).Value = customer.Phone;
        command.Parameters.Add("@address", SqlDbType.NVarChar, 255).Value = customer.Address;
    }

    private static Customer ReadCustomer(SqlDataReader reader)
    {
        return new Customer(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4)
        );
    }
}