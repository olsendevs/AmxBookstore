using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDTO>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
