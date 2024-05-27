using Domain.Entities.Books;
using Domain.Entities.Orders;
using System.Linq.Expressions;


namespace AmxBookstore.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id);
        Task<Order> GetByIdAndSellerIdAsync(Guid id, Guid sellerId);
        Task<Order> GetByIdAndClientIdAsync(Guid id, Guid clientId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetPagedOrdersAsync(int page, int limit, Expression<Func<Order, bool>> filter = null);
        Task<IEnumerable<Order>> GetPagedOrdersForSellerAsync(int page, int limit, Guid sellerId, Expression<Func<Order, bool>> filter = null);
        Task<IEnumerable<Order>> GetPagedOrdersForClientAsync(int page, int limit, Guid clientId, Expression<Func<Order, bool>> filter = null);
        Task AddAsync(Order user);
        Task UpdateAsync(Order user);
        Task DeleteAsync(Guid id);
    }
}
