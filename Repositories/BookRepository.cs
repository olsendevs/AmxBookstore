using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Infrastructure.Persistence;
using Domain.Entities.Books;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace AmxBookstore.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Book> GetByIdAsync(Guid id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.MarkAsDeleted();
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Book>> GetPagedBooksAsync(int page, int limit, Expression<Func<Book, bool>> filter = null)
        {
            var query = _context.Books.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = query.Skip((page - 1) * limit).Take(limit);

            return await query.ToListAsync();
        }

    }
}
