using MediatR;

namespace AmxBookstore.Application.UseCases.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
