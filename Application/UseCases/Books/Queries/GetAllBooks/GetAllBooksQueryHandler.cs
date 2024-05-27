using System.Linq.Expressions;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Application.Filters;
using AmxBookstore.Domain.Interfaces;
using AutoMapper;
using Domain.Entities.Books;
using MediatR;

namespace AmxBookstore.Application.UseCases.Books.Queries.GetAllBooks
{
    public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, IEnumerable<BookDTO>>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public GetAllBooksQueryHandler(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookDTO>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
        {
            var filter = BuildFilter(request.Filter);

            var books = await _bookRepository.GetPagedBooksAsync(request.Page, request.Limit, filter);
            return _mapper.Map<IEnumerable<BookDTO>>(books);
        }

        private Expression<Func<Book, bool>> BuildFilter(BookFilter filter)
        {
            if (filter == null)
                return book => true;

            Expression<Func<Book, bool>> predicate = book => true;

            if (!string.IsNullOrEmpty(filter.Title))
            {
                predicate = CombineExpressions(predicate, book => book.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(filter.Author))
            {
                predicate = CombineExpressions(predicate, book => book.Author.Contains(filter.Author, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.MinPrice.HasValue)
            {
                predicate = CombineExpressions(predicate, book => book.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                predicate = CombineExpressions(predicate, book => book.Price <= filter.MaxPrice.Value);
            }

            if (filter.MinPages.HasValue)
            {
                predicate = CombineExpressions(predicate, book => book.Pages >= filter.MinPages.Value);
            }

            if (filter.MaxPages.HasValue)
            {
                predicate = CombineExpressions(predicate, book => book.Pages <= filter.MaxPages.Value);
            }

            return predicate;
        }

        private static Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var body = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter)
            );

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}
