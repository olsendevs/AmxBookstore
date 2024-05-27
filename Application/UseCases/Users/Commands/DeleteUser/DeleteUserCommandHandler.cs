using MediatR;
using AmxBookstore.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AmxBookstore.Application.UseCases.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public DeleteUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (user.Role != Domain.Entities.Users.Enum.UserRoles.Client && request.UserRole == "Seller")
            {
                throw new BadHttpRequestException("Seller can only update clients");
            }


            await _userRepository.DeleteAsync(user.Id);

            return Unit.Value;
        }
    }
}
