using MediatR;

namespace AmxBookstore.Application.UseCases.Orders.Commands.DeleteOrder
{
    public class DeleteOrderCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
