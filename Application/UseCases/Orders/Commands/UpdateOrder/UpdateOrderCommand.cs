using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Orders.Commands.UpdateOrder
{
    public class UpdateOrderCommand : IRequest<Unit>
    {
        public OrderDTO Order { get; set; }
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
