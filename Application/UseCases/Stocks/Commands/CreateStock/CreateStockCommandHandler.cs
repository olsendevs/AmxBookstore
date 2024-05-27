using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Stocks;
using Microsoft.AspNetCore.Http;

namespace AmxBookstore.Application.UseCases.Stocks.Commands.CreateStock
{
    public class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, Guid>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public CreateStockCommandHandler(IStockRepository stockRepository, IBookRepository bookRepository, IMapper mapper)
        {
            _stockRepository = stockRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateStockCommand request, CancellationToken cancellationToken)
        {
            var stock = _mapper.Map<Stock>(request.Stock);

            var book = await _bookRepository.GetByIdAsync(stock.BookId);

            if (book == null)
            {
                throw new BadHttpRequestException("Book not founded");
            }

            await _stockRepository.AddAsync(stock);
            return stock.Id;
        }
    }
}
