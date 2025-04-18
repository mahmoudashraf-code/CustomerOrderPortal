using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuthService _authService;
    private readonly ICustomerBanService _customerBanService;

    public CustomerController(
        ICustomerRepository customerRepository,
        IAuthService authService,
        ICustomerBanService customerBanService)
    {
        _customerRepository = customerRepository;
        _authService = authService;
        _customerBanService = customerBanService;
    }

    [HttpPost]
    [AllowAnonymous] // Allow anonymous access for customer registration
    public async Task<IActionResult> Create(Customer customer)
    {
        // Check if email already exists
        var existingCustomer = await _customerRepository.GetByEmailAsync(customer.Email);
        if (existingCustomer != null)
        {
            return BadRequest("Email already in use");
        }

        // In production, hash the password before saving
        var result = await _customerRepository.CreateAsync(customer);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize] // Require authentication to get customer details
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost("login")]
    [AllowAnonymous] // Allow anonymous access for login
    public async Task<IActionResult> Login(CustomerLoginRequest request)
    {
        var response = await _authService.AuthenticateAsync(request);
        if (response == null)
        {
            return Unauthorized("Invalid email or password");
        }

        return Ok(response);
    }

    [HttpGet("ban-status")]
    [Authorize]
    public async Task<IActionResult> CheckBanStatus()
    {
        // Extract customer ID from the JWT token
        var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out var customerId))
        {
            return Unauthorized();
        }

        var isBanned = await _customerBanService.IsCustomerBanned(customerId);
        var banStatus = await _customerBanService.GetCustomerBanStatus(customerId);

        return Ok(new
        {
            IsBanned = isBanned,
            BanDetails = banStatus
        });
    }
}
