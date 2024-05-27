using MediatR;
using AmxBookstore.Application.DTOs;
using System.Collections.Generic;

namespace AmxBookstore.Application.UseCases.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<IEnumerable<UserDTO>>
    {
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = 10;
        public Guid UserId { get; set; }
        public string? UserRole { get; set; }
    }
}
