using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;

namespace AmxBookstore.Application.UseCases.Books.Commands.UpdateBook
{
    public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, Unit>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public UpdateBookCommandHandler(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(request.Book.Id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found");
            }

            _mapper.Map(request.Book, book);
            await _bookRepository.UpdateAsync(book);

            return Unit.Value;
        }
    }
}
