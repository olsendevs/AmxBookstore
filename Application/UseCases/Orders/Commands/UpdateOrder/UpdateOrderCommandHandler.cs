using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using AmxBookstore.Domain.Entities.Orders;

namespace AmxBookstore.Application.UseCases.Orders.Commands.UpdateOrder
{
    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Unit>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public UpdateOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.Order.Products != null)
            {
                throw new BadHttpRequestException("Cannot update products from a order");
            }

            var order = request.UserRole == "Admin" 
                ? await _orderRepository.GetByIdAsync(request.Order.Id) 
                : await _orderRepository.GetByIdAndSellerIdAsync(request.Order.Id, request.UserId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            order.Update(
                request.Order.TotalAmount, 
                request.Order.SellerId, 
                request.Order.ClientId, 
                (OrderStatus)Enum.Parse(typeof(OrderStatus), request.Order.Status)
            );

            await _orderRepository.UpdateAsync(order);

            return Unit.Value;
        }
    }
}
