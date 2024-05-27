using Domain.Entities.Stocks;


namespace AmxBookstore.Domain.Interfaces
{
    public interface IStockRepository
    {
        Task<Stock> GetByIdAsync(Guid id);

        Task<Stock> GetByProductIdAsync(Guid id);
        Task<IEnumerable<Stock>> GetAllAsync();
        Task<IEnumerable<Stock>> GetPagedStocksAsync(int page, int limit);
        Task AddAsync(Stock user);
        Task UpdateAsync(Stock user);
        Task DeleteAsync(Guid id);
    }
}
