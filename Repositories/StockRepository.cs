using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Infrastructure.Persistence;
using Domain.Entities.Stocks;
using Microsoft.EntityFrameworkCore;


namespace AmxBookstore.Infrastructure.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly AppDbContext _context;

        public StockRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Stock> GetByIdAsync(Guid id)
        {
            return await _context.Stocks.FindAsync(id);
        }

        public async Task<Stock> GetByProductIdAsync(Guid id)
        {
            return await _context.Stocks.Where((stock) => stock.BookId == id).FirstAsync();
        }

        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            return await _context.Stocks.ToListAsync();
        }

        public async Task AddAsync(Stock stock)
        {
            await _context.Stocks.AddAsync(stock);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stock stock)
        {
            _context.Stocks.Update(stock);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock != null)
            {
                stock.MarkAsDeleted();
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Stock>> GetPagedStocksAsync(int page, int limit)
        {
            var query = _context.Stocks.AsQueryable();

            query = query.Skip((page - 1) * limit).Take(limit);

            return await query.ToListAsync();
        }
    }
}
