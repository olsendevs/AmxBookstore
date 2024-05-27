using MediatR;
using AmxBookstore.Application.DTOs;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Domain.Entities.Orders;

namespace AmxBookstore.Application.UseCases.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDTO>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper, IMemoryCache cache)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<OrderDTO> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"Order_{request.UserRole}_{request.UserId}_{request.Id}";

            if (_cache.TryGetValue(cacheKey, out OrderDTO orderDto))
            {
                return orderDto;
            }

            var roles = new Dictionary<string, Func<Task<Order>>>
            {
                { "Client", async () => await _orderRepository.GetByIdAndClientIdAsync(request.Id, request.UserId) },
                { "Seller", async () => await _orderRepository.GetByIdAndSellerIdAsync(request.Id, request.UserId) },
                { "Admin", async () => await _orderRepository.GetByIdAsync(request.Id) }
            };

            Order order = null;

            if (roles.ContainsKey(request.UserRole))
            {
                order = await roles[request.UserRole]();
            }

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            orderDto = _mapper.Map<OrderDTO>(order);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30), 
                SlidingExpiration = TimeSpan.FromSeconds(15) 
            };

            _cache.Set(cacheKey, orderDto, cacheOptions);

            return orderDto;
        }
    }
}
