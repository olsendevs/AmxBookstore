using MediatR;
using AmxBookstore.Domain.Interfaces;

namespace AmxBookstore.Application.UseCases.Orders.Commands.DeleteOrder
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Unit>
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = request.UserRole == "Admin"
                ? await _orderRepository.GetByIdAsync(request.Id)
                : await _orderRepository.GetByIdAndSellerIdAsync(request.Id, request.UserId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            await _orderRepository.DeleteAsync(order.Id);

            return Unit.Value;
        }
    }
}
