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
    public class CustomersController : ControllerBase
    {
        private readonly FlowerDeliveryContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(FlowerDeliveryContext context, IMapper mapper, ILogger<CustomersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Customers.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(c =>
                        c.Name.Contains(search) ||
                        c.Phone.Contains(search) ||
                        c.Email.Contains(search));
                }

                var customers = await query.OrderBy(c => c.Name).ToListAsync();
                return Ok(_mapper.Map<List<CustomerDto>>(customers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиентов");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

       
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);

                if (customer == null)
                {
                    return NotFound(new { Message = $"Клиент с ID {id} не найден" });
                }

                return Ok(_mapper.Map<CustomerDto>(customer));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиента {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CustomerCreateDto customerCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

               
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Phone == customerCreateDto.Phone);

                if (existingCustomer != null)
                {
                    return Conflict(new { Message = "Клиент с таким телефоном уже существует" });
                }

                var customer = _mapper.Map<Customer>(customerCreateDto);
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                var customerDto = _mapper.Map<CustomerDto>(customer);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerID }, customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании клиента");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerCreateDto customerCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { Message = $"Клиент с ID {id} не найден" });
                }

               
                var phoneExists = await _context.Customers
                    .AnyAsync(c => c.Phone == customerCreateDto.Phone && c.CustomerID != id);

                if (phoneExists)
                {
                    return Conflict(new { Message = "Клиент с таким телефоном уже существует" });
                }

                _mapper.Map(customerCreateDto, customer);
                _context.Entry(customer).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(id))
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
                _logger.LogError(ex, "Ошибка при обновлении клиента {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { Message = $"Клиент с ID {id} не найден" });
                }

                
                var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerID == id);
                if (hasOrders)
                {
                    return BadRequest(new
                    {
                        Message = "Невозможно удалить клиента, у которого есть заказы",
                        Details = "Сначала удалите все заказы клиента"
                    });
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении клиента {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("{id}/orders")]
        public async Task<ActionResult> GetCustomerOrders(int id)
        {
            try
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.CustomerID == id);
                if (!customerExists)
                {
                    return NotFound(new { Message = $"Клиент с ID {id} не найден" });
                }

                var orders = await _context.Orders
                    .Where(o => o.CustomerID == id)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Flower)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderDtos = orders.Select(o => new
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    DeliveryAddress = o.DeliveryAddress,
                    RecipientName = o.RecipientName,
                    ItemCount = o.OrderItems.Count
                }).ToList();

                return Ok(new
                {
                    CustomerID = id,
                    TotalOrders = orders.Count,
                    TotalSpent = orders.Sum(o => o.TotalAmount),
                    Orders = orderDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказов клиента {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }
    }
}