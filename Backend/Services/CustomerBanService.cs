using Backend.Models;

namespace Backend.Services;

public class CustomerBanService : ICustomerBanService
{
    private readonly Dictionary<int, List<OrderDeletion>> _deletionsByCustomer = new();
    private readonly Dictionary<int, DateTime> _bannedCustomers = new();
    
    public Task RecordOrderDeletion(int customerId, int orderId, DateTime orderCreationDate)
    {
        var deletion = new OrderDeletion
        {
            CustomerId = customerId,
            OrderId = orderId,
            OrderCreationDate = orderCreationDate,
            DeletionDate = DateTime.Now
        };
        
        if (!_deletionsByCustomer.ContainsKey(customerId))
        {
            _deletionsByCustomer[customerId] = new List<OrderDeletion>();
        }
        
        _deletionsByCustomer[customerId].Add(deletion);
        
        // Check if customer should be banned
        CheckAndBanCustomer(customerId);
        
        return Task.CompletedTask;
    }
    
    public Task<bool> IsCustomerBanned(int customerId)
    {
        if (_bannedCustomers.TryGetValue(customerId, out var banEndTime))
        {
            if (DateTime.Now < banEndTime)
            {
                return Task.FromResult(true);
            }
            
            // Ban period expired, remove from banned list
            _bannedCustomers.Remove(customerId);
        }
        
        return Task.FromResult(false);
    }
    
    public Task<CustomerBanStatus> GetCustomerBanStatus(int customerId)
    {
        if (_bannedCustomers.TryGetValue(customerId, out var banEndTime) && DateTime.Now < banEndTime)
        {
            var banStatus = new CustomerBanStatus
            {
                CustomerId = customerId,
                BanStartTime = banEndTime.AddHours(-6), // Ban is for 6 hours
                BanEndTime = banEndTime
            };
            return Task.FromResult(banStatus);
        }
        
        return Task.FromResult<CustomerBanStatus>(null);
    }
    
    private void CheckAndBanCustomer(int customerId)
    {
        if (!_deletionsByCustomer.ContainsKey(customerId)) return;
        
        var today = DateTime.Now.Date;
        var deletionsToday = _deletionsByCustomer[customerId]
            .Where(d => d.DeletionDate.Date == today && d.OrderCreationDate.Date == d.DeletionDate.Date)
            .ToList();
            
        if (deletionsToday.Count >= 3)
        {
            // Ban customer for 6 hours
            _bannedCustomers[customerId] = DateTime.Now.AddHours(6);
        }
    }
}
