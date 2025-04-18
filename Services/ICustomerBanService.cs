using Backend.Models;

namespace Backend.Services;

public interface ICustomerBanService
{
    Task RecordOrderDeletion(int customerId, int orderId, DateTime orderCreationDate);
    Task<bool> IsCustomerBanned(int customerId);
    Task<CustomerBanStatus> GetCustomerBanStatus(int customerId);
}
