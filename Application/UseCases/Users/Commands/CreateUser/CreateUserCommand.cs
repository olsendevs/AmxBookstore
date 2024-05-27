using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<Guid>
    {
        public UserDTO User { get; set; }
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
