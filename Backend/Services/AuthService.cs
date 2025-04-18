using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;
using Backend.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class AuthService : IAuthService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IConfiguration _configuration;

    public AuthService(ICustomerRepository customerRepository, IConfiguration configuration)
    {
        _customerRepository = customerRepository;
        _configuration = configuration;
    }

    public async Task<CustomerLoginResponse?> AuthenticateAsync(CustomerLoginRequest request)
    {
        var customer = await _customerRepository.GetByEmailAsync(request.Email);
        
        // In a real app, verify hashed password
        if (customer == null || customer.Password != request.Password)
        {
            return null;
        }

        // Authentication successful
        var token = GenerateJwtToken(customer);
        
        return new CustomerLoginResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Token = token
        };
    }

    public string GenerateJwtToken(Customer customer)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "defaultSecretKey12345678901234567890"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Email, customer.Email)
        };
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "customerOrderPortal",
            audience: _configuration["Jwt:Audience"] ?? "customerOrderPortalClients",
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: credentials);
            
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
