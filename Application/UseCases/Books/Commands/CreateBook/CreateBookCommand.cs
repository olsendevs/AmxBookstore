using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Books.Commands.CreateBook
{
    public class CreateBookCommand : IRequest<Guid>
    {
        public BookDTO Book { get; set; }
    }
}
