using Backend.Models;

namespace Backend.Services;

public interface IAuthService
{
    Task<CustomerLoginResponse?> AuthenticateAsync(CustomerLoginRequest request);
    string GenerateJwtToken(Customer customer);
}
