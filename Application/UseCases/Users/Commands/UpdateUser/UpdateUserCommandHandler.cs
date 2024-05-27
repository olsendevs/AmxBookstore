using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AmxBookstore.Application.UseCases.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.User.Id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if ((user.Role != Domain.Entities.Users.Enum.UserRoles.Client || request.User.Role != Domain.Entities.Users.Enum.UserRoles.Client.ToString()) && request.UserRole == "Seller")
            {
                throw new BadHttpRequestException("Seller can only update clients");
            }

            _mapper.Map(request.User, user);
            await _userRepository.UpdateAsync(user);

            return Unit.Value;
        }
    }
}
