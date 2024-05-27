using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<Guid>
    {
        public OrderDTO Order { get; set; }
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
