using MediatR;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Application.Filters;

namespace AmxBookstore.Application.UseCases.Books.Queries.GetAllBooks
{
    public class GetAllBooksQuery : IRequest<IEnumerable<BookDTO>>
    {
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = 10;
        public BookFilter Filter { get; set; }
    }
}
