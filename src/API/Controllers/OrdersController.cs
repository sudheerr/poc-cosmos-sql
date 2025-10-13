using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderRepository _orderRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderRepository orderRepository,
        CustomerRepository customerRepository,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync(cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, "An error occurred while retrieving orders");
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> GetById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
                return NotFound($"Order with ID {id} not found");

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "An error occurred while retrieving the order");
        }
    }

    /// <summary>
    /// Get orders by customer ID
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetByCustomerId(
        string customerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while retrieving orders");
        }
    }

    /// <summary>
    /// Get orders by status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetByStatus(
        string status,
        CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _orderRepository.GetByStatusAsync(status, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by status {Status}", status);
            return StatusCode(500, "An error occurred while retrieving orders");
        }
    }

    /// <summary>
    /// Get pending orders
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetPendingOrders(
        CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _orderRepository.GetPendingOrdersAsync(cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending orders");
            return StatusCode(500, "An error occurred while retrieving pending orders");
        }
    }

    /// <summary>
    /// Get orders within date range
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest("Start date must be before end date");

            var orders = await _orderRepository.GetOrdersByDateRangeAsync(
                startDate, endDate, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by date range");
            return StatusCode(500, "An error occurred while retrieving orders");
        }
    }

    /// <summary>
    /// Get total revenue (optionally within date range)
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTotalRevenue(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var totalRevenue = await _orderRepository.GetTotalRevenueAsync(
                startDate, endDate, cancellationToken);

            return Ok(new
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                Currency = "USD"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total revenue");
            return StatusCode(500, "An error occurred while calculating revenue");
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> Create(
        [FromBody] Order order,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify customer exists
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
            if (customer == null)
                return NotFound($"Customer with ID {order.CustomerId} not found");

            // Calculate total amount
            order.TotalAmount = order.Items.Sum(item => item.TotalPrice);
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Pending";

            var created = await _orderRepository.AddAsync(order, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "An error occurred while creating the order");
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> UpdateStatus(
        string id,
        [FromBody] string status,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
                return NotFound($"Order with ID {id} not found");

            order.Status = status;
            var updated = await _orderRepository.UpdateAsync(order, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status {OrderId}", id);
            return StatusCode(500, "An error occurred while updating the order status");
        }
    }

    /// <summary>
    /// Update an existing order
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> Update(
        string id,
        [FromBody] Order order,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound($"Order with ID {id} not found");

            order.Id = id;
            order.CreatedAt = existing.CreatedAt;
            order.TotalAmount = order.Items.Sum(item => item.TotalPrice);

            var updated = await _orderRepository.UpdateAsync(order, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", id);
            return StatusCode(500, "An error occurred while updating the order");
        }
    }

    /// <summary>
    /// Delete an order
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _orderRepository.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound($"Order with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            return StatusCode(500, "An error occurred while deleting the order");
        }
    }

    /// <summary>
    /// Complex LINQ query example - Get high value orders
    /// </summary>
    [HttpGet("high-value")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetHighValueOrders(
        [FromQuery] decimal minAmount = 1000,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Using LINQ with repository
            var orders = await _orderRepository.QueryAsync(query =>
                query.Where(o => o.TotalAmount >= minAmount)
                     .Where(o => o.Status == "Completed")
                     .OrderByDescending(o => o.TotalAmount),
                cancellationToken);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving high value orders");
            return StatusCode(500, "An error occurred while retrieving high value orders");
        }
    }
}
