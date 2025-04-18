using Backend.Models;

namespace Backend.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();
    private int _nextId = 1;
    private int _nextItemId = 1;

    public OrderRepository()
    {
        // Add some sample data
        var order = new Order
        {
            Id = _nextId++,
            CustomerId = 1,
            OrderDate = DateTime.Now.AddDays(-5),
            TotalAmount = 99.99m,
            Status = "Shipped",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = _nextItemId++,
                    ProductName = "Product A",
                    Quantity = 2,
                    UnitPrice = 49.99m
                }
            }
        };
        _orders.Add(order);
    }

    public Task<IEnumerable<Order>> GetAllAsync()
    {
        return Task.FromResult(_orders.AsEnumerable());
    }

    public Task<Order?> GetByIdAsync(int id)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    }

    public Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)
    {
        return Task.FromResult(_orders.Where(o => o.CustomerId == customerId));
    }

    public Task<Order> CreateAsync(Order order)
    {
        order.Id = _nextId++;
        foreach (var item in order.Items)
        {
            item.Id = _nextItemId++;
        }
        _orders.Add(order);
        return Task.FromResult(order);
    }

    public Task<bool> DeleteAsync(int id, int customerId)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id && o.CustomerId == customerId);
        if (order == null)
        {
            return Task.FromResult(false);
        }
        
        _orders.Remove(order);
        return Task.FromResult(true);
    }
}
