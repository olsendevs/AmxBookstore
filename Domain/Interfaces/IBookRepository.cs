using Domain.Entities.Books;
using System.Linq.Expressions;


namespace AmxBookstore.Domain.Interfaces
{
    public interface IBookRepository
    {
        Task<Book> GetByIdAsync(Guid id);
        Task<IEnumerable<Book>> GetAllAsync();
        Task<IEnumerable<Book>> GetPagedBooksAsync(int page, int limit, Expression<Func<Book, bool>> filter = null);
        Task AddAsync(Book user);
        Task UpdateAsync(Book user);
        Task DeleteAsync(Guid id);
    }
}
