using Backend.Models;

namespace Backend.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
    Task<Order> CreateAsync(Order order);
    Task<bool> DeleteAsync(int id, int customerId);
}
