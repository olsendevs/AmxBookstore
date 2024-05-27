using MediatR;
using AmxBookstore.Domain.Interfaces;

namespace AmxBookstore.Application.UseCases.Stocks.Commands.DeleteStock
{
    public class DeleteStockCommandHandler : IRequestHandler<DeleteStockCommand, Unit>
    {
        private readonly IStockRepository _orderRepository;

        public DeleteStockCommandHandler(IStockRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Unit> Handle(DeleteStockCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id);
            if (order == null)
            {
                throw new KeyNotFoundException("Stock not found");
            }

            await _orderRepository.DeleteAsync(order.Id);

            return Unit.Value;
        }
    }
}
