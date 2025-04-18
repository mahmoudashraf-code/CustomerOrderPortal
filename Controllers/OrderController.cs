using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Add this to require authentication for all endpoints in this controller
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerBanService _customerBanService;

    public OrderController(
        IOrderRepository orderRepository, 
        ICustomerRepository customerRepository,
        ICustomerBanService customerBanService)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _customerBanService = customerBanService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderRepository.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        // Check if customer is banned
        if (await _customerBanService.IsCustomerBanned(order.CustomerId))
        {
            return BadRequest("You are temporarily banned from placing orders due to excessive order cancellations");
        }
        
        // Verify customer exists
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer == null)
        {
            return BadRequest("Customer not found");
        }

        // Set order date if not provided
        if (order.OrderDate == default)
        {
            order.OrderDate = DateTime.Now;
        }

        var result = await _orderRepository.CreateAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        // Verify customer exists
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            return BadRequest("Customer not found");
        }

        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return Ok(orders);
    }

    [HttpDelete("{id}/customer/{customerId}")]
    public async Task<IActionResult> Delete(int id, int customerId)
    {
        // Get the order first to check its creation date
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null || order.CustomerId != customerId)
        {
            return NotFound();
        }
        
        var success = await _orderRepository.DeleteAsync(id, customerId);
        if (!success)
        {
            return NotFound();
        }
        
        // Record the deletion for monitoring
        await _customerBanService.RecordOrderDeletion(customerId, id, order.OrderDate);
        
        return NoContent();
    }
}
