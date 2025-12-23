using Microsoft.EntityFrameworkCore;
using FlowerDeliveryAPI.Data;
using FlowerDeliveryAPI.Repositories;
using FlowerDeliveryAPI.Models;

namespace FlowerDeliveryApi.Repositories
{
    public class CustomerRepository : IRepository<Customer>
    {
        private readonly FlowerDeliveryContext _context;

        public CustomerRepository(FlowerDeliveryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
            => await _context.Customers.ToListAsync();

        public async Task<Customer?> GetByIdAsync(int id)
            => await _context.Customers.FindAsync(id);

        public async Task<Customer> AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Customers.AnyAsync(e => e.CustomerID == id);
    }
}