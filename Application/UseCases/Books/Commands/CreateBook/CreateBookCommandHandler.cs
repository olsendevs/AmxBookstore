using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Books;
using Domain.Entities.Stocks;

namespace AmxBookstore.Application.UseCases.Books.Commands.CreateBook
{
    public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Guid>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;

        public CreateBookCommandHandler(IBookRepository bookRepository, IStockRepository stockRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateBookCommand request, CancellationToken cancellationToken)
        {
            var book = _mapper.Map<Book>(request.Book);
            await _bookRepository.AddAsync(book);

            await _stockRepository.AddAsync(new Stock(book.Id, 1));
            
            return book.Id;
        }
    }
}
