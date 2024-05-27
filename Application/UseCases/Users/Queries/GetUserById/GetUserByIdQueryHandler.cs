using MediatR;
using AmxBookstore.Application.DTOs;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AmxBookstore.Application.UseCases.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper, IMemoryCache cache)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<UserDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"User_{request.Id}";

            if (_cache.TryGetValue(cacheKey, out UserDTO userDto))
            {
                return userDto;
            }

            var user = await _userRepository.GetByIdAsync(request.Id);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            userDto = _mapper.Map<UserDTO>(user);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                SlidingExpiration = TimeSpan.FromSeconds(15)
            };

            _cache.Set(cacheKey, userDto, cacheOptions);

            return userDto;
        }
    }
}
