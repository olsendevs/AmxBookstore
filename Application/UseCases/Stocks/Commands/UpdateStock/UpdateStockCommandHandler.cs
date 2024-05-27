using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;

namespace AmxBookstore.Application.UseCases.Stocks.Commands.UpdateStock
{
    public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, Unit>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;

        public UpdateStockCommandHandler(IStockRepository stockRepository, IMapper mapper)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            var stock = await _stockRepository.GetByIdAsync(request.Id);
            if (stock == null)
            {
                throw new KeyNotFoundException("Stock not found");
            }

            stock.UpdateQuantity(request.Quantity);

            await _stockRepository.UpdateAsync(stock);

            return Unit.Value;
        }
    }
}
