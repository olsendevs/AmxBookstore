using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Books.Commands.UpdateBook
{
    public class UpdateBookCommand : IRequest<Unit>
    {
        public BookDTO Book { get; set; }
    }
}
