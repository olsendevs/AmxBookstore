using MediatR;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Application.Filters;

namespace AmxBookstore.Application.UseCases.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersQuery : IRequest<IEnumerable<OrderDTO>>
    {
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
        public OrderFilter Filter { get; set; }
        public int Page { get; set; } = 0;
        public int Limit{ get; set; } = 10;

    }
}
