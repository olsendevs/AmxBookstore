using MediatR;
using AmxBookstore.Application.DTOs;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Http;

namespace AmxBookstore.Application.UseCases.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper, IMemoryCache cache)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<UserDTO>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"GetAllUsers-{request.Page}-{request.Limit}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<UserDTO> usersDto))
            {
                return usersDto;
            }

            var roles = new Dictionary<string, Func<Task<IEnumerable<User>>>>
            {
                { "Seller", async () => await _userRepository.GetPagedClientsAsync(request.Page, request.Limit) },
                { "Admin", async () => await _userRepository.GetPagedUsersAsync(request.Page, request.Limit) }
            };


            if (!roles.ContainsKey(request.UserRole))
            {
                throw new BadHttpRequestException("User withou role");
                
            }

            var users = await roles[request.UserRole]();

            usersDto = _mapper.Map<IEnumerable<UserDTO>>(users);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5),
                SlidingExpiration = TimeSpan.FromSeconds(1)
            };
            if (usersDto.Count() > 0)
            {
                _cache.Set(cacheKey, usersDto, cacheOptions);

            }

            return usersDto;
        }
    }
}
