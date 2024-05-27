using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Infrastructure.Persistence;
using Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace AmxBookstore.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> GetByIdAsync(Guid id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task<Order> GetByIdAndSellerIdAsync(Guid id, Guid sellerId)
        {
            var query = _context.Orders.AsQueryable();

            query = query.Where(o => o.Id == id && o.SellerId == sellerId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Order> GetByIdAndClientIdAsync(Guid id, Guid clientId)
        {
            var query = _context.Orders.AsQueryable();

            query = query.Where(o => o.Id == id && o.ClientId == clientId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Order> GetByIdAsync(Guid id, Guid userId, string? userRole)
        {
            var query = _context.Orders.AsQueryable();

            query = query.Where(o => o.Id == id);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.MarkAsDeleted();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Order>> GetPagedOrdersForClientAsync(int page, int limit, Guid clientId, Expression<Func<Order, bool>> filter = null)
        {
            var query = _context.Orders.Where(o => o.ClientId == clientId).AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter).AsQueryable();
            }

            query = query.Skip((page - 1) * limit).Take(limit);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetPagedOrdersForSellerAsync(int page, int limit, Guid sellerId, Expression<Func<Order, bool>> filter = null)
        {
            var query = _context.Orders.Where(o => o.SellerId == sellerId).AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter).AsQueryable();
            }

            query = query.Skip((page - 1) * limit).Take(limit);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetPagedOrdersAsync(int page, int limit, Expression<Func<Order, bool>> filter = null)
        {
            var query = _context.Orders.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter).AsQueryable();
            }

            query = query.Skip((page - 1) * limit).Take(limit);

            return await query.ToListAsync();
        }
    }
}
