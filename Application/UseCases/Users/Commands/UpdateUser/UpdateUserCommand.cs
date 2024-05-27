using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<Unit>
    {
        public UserDTO User { get; set; }
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
