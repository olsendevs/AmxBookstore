using MediatR;
using AmxBookstore.Domain.Interfaces;

namespace AmxBookstore.Application.UseCases.Books.Commands.DeleteBook
{
    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, Unit>
    {
        private readonly IBookRepository _bookRepository;

        public DeleteBookCommandHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Unit> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(request.Id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found");
            }

            await _bookRepository.DeleteAsync(book.Id);

            return Unit.Value;
        }
    }
}
