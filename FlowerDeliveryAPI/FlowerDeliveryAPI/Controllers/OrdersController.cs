using AutoMapper;
using FlowerDeliveryAPI.Data;
using FlowerDeliveryAPI.DTOs;
using FlowerDeliveryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerDeliveryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly FlowerDeliveryContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(FlowerDeliveryContext context, IMapper mapper, ILogger<OrdersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? sortBy = "date",
            [FromQuery] bool? descending = true)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Flower)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(o => o.Status == status);
                }

                if (customerId.HasValue)
                {
                    query = query.Where(o => o.CustomerID == customerId.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= endDate.Value);
                }

                
                query = sortBy?.ToLower() switch
                {
                    "amount" => descending == true
                        ? query.OrderByDescending(o => o.TotalAmount)
                        : query.OrderBy(o => o.TotalAmount),
                    "status" => descending == true
                        ? query.OrderByDescending(o => o.Status)
                        : query.OrderBy(o => o.Status),
                    _ => descending == true
                        ? query.OrderByDescending(o => o.OrderDate)
                        : query.OrderBy(o => o.OrderDate)
                };

                var orders = await query.ToListAsync();
                return Ok(_mapper.Map<List<OrderDto>>(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказов");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

       
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Flower)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                {
                    return NotFound(new { Message = $"Заказ с ID {id} не найден" });
                }

                return Ok(_mapper.Map<OrderDto>(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказа {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(OrderCreateDto orderCreateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                
                var customer = await _context.Customers.FindAsync(orderCreateDto.CustomerID);
                if (customer == null)
                {
                    return BadRequest(new { Message = $"Клиент с ID {orderCreateDto.CustomerID} не найден" });
                }

                var order = new Order
                {
                    CustomerID = orderCreateDto.CustomerID,
                    DeliveryAddress = orderCreateDto.DeliveryAddress,
                    RecipientName = orderCreateDto.RecipientName,
                    RecipientPhone = orderCreateDto.RecipientPhone,
                    DeliveryDate = orderCreateDto.DeliveryDate,
                    DeliveryTime = orderCreateDto.DeliveryTime,
                    Notes = orderCreateDto.Notes,
                    Status = "Новый",
                    OrderDate = DateTime.Now
                };

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                
                foreach (var itemDto in orderCreateDto.Items)
                {
                    var flower = await _context.Flowers.FindAsync(itemDto.FlowerID);
                    if (flower == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            Message = $"Цветок с ID {itemDto.FlowerID} не найден"
                        });
                    }

                    if (!flower.IsAvailable || flower.InStock < itemDto.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            Message = $"Недостаточно цветов '{flower.Name}' на складе",
                            Available = flower.InStock,
                            Requested = itemDto.Quantity
                        });
                    }

                    var orderItem = new OrderItem
                    {
                        FlowerID = itemDto.FlowerID,
                        Quantity = itemDto.Quantity,
                        Price = flower.Price
                    };

                    totalAmount += flower.Price * itemDto.Quantity;
                    orderItems.Add(orderItem);

                    
                    flower.InStock -= itemDto.Quantity;
                    if (flower.InStock == 0)
                    {
                        flower.IsAvailable = false;
                    }
                }

                order.TotalAmount = totalAmount;
                order.OrderItems = orderItems;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Создан новый заказ #{OrderId} на сумму {TotalAmount}",
                    order.OrderID, order.TotalAmount);

               
                var createdOrder = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Flower)
                    .FirstOrDefaultAsync(o => o.OrderID == order.OrderID);

                var orderDto = _mapper.Map<OrderDto>(createdOrder!);
                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID }, orderDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при создании заказа");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, OrderUpdateDto orderUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { Message = $"Заказ с ID {id} не найден" });
                }

                _mapper.Map(orderUpdateDto, order);
                _context.Entry(order).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении заказа {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Flower)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                {
                    return NotFound(new { Message = $"Заказ с ID {id} не найден" });
                }

                order.Status = status;
                await _context.SaveChangesAsync();

                return Ok(_mapper.Map<OrderDto>(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статуса заказа {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                {
                    return NotFound(new { Message = $"Заказ с ID {id} не найден" });
                }

             
                foreach (var item in order.OrderItems)
                {
                    var flower = await _context.Flowers.FindAsync(item.FlowerID);
                    if (flower != null)
                    {
                        flower.InStock += item.Quantity;
                        flower.IsAvailable = true;
                    }
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Удален заказ #{OrderId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при удалении заказа {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

       
        [HttpGet("statistics/daily")]
        public async Task<ActionResult> GetDailyStatistics([FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;
                var nextDate = targetDate.AddDays(1);

                var orders = await _context.Orders
                    .Where(o => o.OrderDate >= targetDate && o.OrderDate < nextDate)
                    .Include(o => o.OrderItems)
                    .ToListAsync();

                var stats = new
                {
                    Date = targetDate,
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Sum(o => o.TotalAmount),
                    AverageOrderValue = orders.Count > 0 ? orders.Average(o => o.TotalAmount) : 0,
                    StatusDistribution = orders
                        .GroupBy(o => o.Status)
                        .Select(g => new
                        {
                            Status = g.Key,
                            Count = g.Count(),
                            Revenue = g.Sum(o => o.TotalAmount)
                        })
                        .ToList()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

      
        [HttpGet("revenue")]
        public async Task<ActionResult> GetRevenueStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= endDate.Value);
                }

                var revenue = await query.SumAsync(o => o.TotalAmount);
                var orderCount = await query.CountAsync();

                return Ok(new
                {
                    TotalRevenue = revenue,
                    OrderCount = orderCount,
                    AverageRevenuePerOrder = orderCount > 0 ? revenue / orderCount : 0,
                    DateRange = new
                    {
                        Start = startDate?.ToString("yyyy-MM-dd") ?? "Все время",
                        End = endDate?.ToString("yyyy-MM-dd") ?? "Все время"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики выручки");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}