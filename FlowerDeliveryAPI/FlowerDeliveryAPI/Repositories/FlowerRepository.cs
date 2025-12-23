using Microsoft.EntityFrameworkCore;
using FlowerDeliveryAPI.Data;
using FlowerDeliveryAPI.Repositories;
using FlowerDeliveryAPI.Models;

namespace FlowerDeliveryApi.Repositories
{
    public class FlowerRepository : IRepository<Flower>
    {
        private readonly FlowerDeliveryContext _context;

        public FlowerRepository(FlowerDeliveryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Flower>> GetAllAsync()
            => await _context.Flowers.ToListAsync();

        public async Task<Flower?> GetByIdAsync(int id)
            => await _context.Flowers.FindAsync(id);

        public async Task<Flower> AddAsync(Flower flower)
        {
            _context.Flowers.Add(flower);
            await _context.SaveChangesAsync();
            return flower;
        }

        public async Task<Flower> UpdateAsync(Flower flower)
        {
            _context.Entry(flower).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return flower;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var flower = await _context.Flowers.FindAsync(id);
            if (flower == null) return false;

            _context.Flowers.Remove(flower);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Flowers.AnyAsync(e => e.FlowerID == id);

        public async Task<IEnumerable<Flower>> GetAvailableAsync()
            => await _context.Flowers
                .Where(f => f.IsAvailable && f.InStock > 0)
                .ToListAsync();
    }
}