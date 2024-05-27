using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Http;

namespace AmxBookstore.Application.UseCases.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CreateUserCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<User>(request.User);

            if (user.Role != Domain.Entities.Users.Enum.UserRoles.Client && request.UserRole == "Seller")
            {
                throw new BadHttpRequestException("Seller can only create clients");
            }

            await _userRepository.AddAsync(user, request.User.Password);
            return user.Id;
        }
    }
}
