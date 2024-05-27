using System.Linq.Expressions;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Application.Filters;
using AmxBookstore.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Domain.Entities.Orders;

namespace AmxBookstore.Application.UseCases.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDTO>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetAllOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper, IMemoryCache cache)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<OrderDTO>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"Orders_{request.UserRole}_{request.UserId}_{request.Page}_{request.Limit}_{request.Filter?.GetHashCode()}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<OrderDTO> ordersDto))
            {
                return ordersDto;
            }

            var filter = BuildFilter(request.Filter);

            var roles = new Dictionary<string, Func<Task<IEnumerable<Order>>>>
            {
                { "Client", async () => await _orderRepository.GetPagedOrdersForClientAsync(request.Page, request.Limit, request.UserId, filter) },
                { "Seller", async () => await _orderRepository.GetPagedOrdersForSellerAsync(request.Page, request.Limit, request.UserId, filter) },
                { "Admin", async () => await _orderRepository.GetPagedOrdersAsync(request.Page, request.Limit, filter) }
            };

            IEnumerable<Order> orders = Enumerable.Empty<Order>();

            if (roles.ContainsKey(request.UserRole))
            {
                orders = await roles[request.UserRole]();
            }

            ordersDto = _mapper.Map<IEnumerable<OrderDTO>>(orders);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                SlidingExpiration = TimeSpan.FromSeconds(15)
            };

            _cache.Set(cacheKey, ordersDto, cacheOptions);

            return ordersDto;
        }

        private Expression<Func<Order, bool>> BuildFilter(OrderFilter filter)
        {
            if (filter == null)
                return order => true;

            Expression<Func<Order, bool>> predicate = order => true;

            if (filter.StartDate.HasValue)
            {
                predicate = CombineExpressions(predicate, order => order.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                predicate = CombineExpressions(predicate, order => order.CreatedAt <= filter.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                predicate = CombineExpressions(predicate, order => order.Status.ToString() == filter.Status);
            }

            if (filter.MinTotal.HasValue)
            {
                predicate = CombineExpressions(predicate, order => order.TotalAmount >= filter.MinTotal.Value);
            }

            if (filter.MaxTotal.HasValue)
            {
                predicate = CombineExpressions(predicate, order => order.TotalAmount <= filter.MaxTotal.Value);
            }

            return predicate;
        }

        private static Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var body = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter)
            );

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}
