using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<OrderDTO>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
