using MediatR;
using AmxBookstore.Application.DTOs;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AmxBookstore.Application.UseCases.Stocks.Queries.GetStockById
{
    public class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, StockDTO>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetStockByIdQueryHandler(IStockRepository stockRepository, IMapper mapper, IMemoryCache cache)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<StockDTO> Handle(GetStockByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"Stock_{request.Id}";

            if (_cache.TryGetValue(cacheKey, out StockDTO stockDto))
            {
                return stockDto;
            }

            var stock = await _stockRepository.GetByIdAsync(request.Id);

            stockDto = _mapper.Map<StockDTO>(stock);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30), 
                SlidingExpiration = TimeSpan.FromSeconds(15) 
            };

            _cache.Set(cacheKey, stockDto, cacheOptions);

            return stockDto;
        }
    }
}
