using Backend.Models;

namespace Backend.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new();
    private int _nextId = 1;

    public CustomerRepository()
    {
        // Add some sample data
        _customers.Add(new Customer
        {
            Id = _nextId++,
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password123" 
        });
    }

    public Task<IEnumerable<Customer>> GetAllAsync()
    {
        return Task.FromResult(_customers.AsEnumerable());
    }

    public Task<Customer?> GetByIdAsync(int id)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task<Customer?> GetByEmailAsync(string email)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<Customer> CreateAsync(Customer customer)
    {
        customer.Id = _nextId++;
        _customers.Add(customer);
        return Task.FromResult(customer);
    }
}
