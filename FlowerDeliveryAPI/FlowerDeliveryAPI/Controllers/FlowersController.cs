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
    public class FlowersController : ControllerBase
    {
        private readonly FlowerDeliveryContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<FlowersController> _logger;

        public FlowersController(FlowerDeliveryContext context, IMapper mapper, ILogger<FlowersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlowerDto>>> GetFlowers()
        {
            try
            {
                var flowers = await _context.Flowers.ToListAsync();
                var flowerDtos = _mapper.Map<IEnumerable<FlowerDto>>(flowers);
                return Ok(flowerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении цветов");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}