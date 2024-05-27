using MediatR;
using AmxBookstore.Application.DTOs;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AmxBookstore.Application.UseCases.Stocks.Queries.GetAllStocks
{
    public class GetAllStocksQueryHandler : IRequestHandler<GetAllStocksQuery, IEnumerable<StockDTO>>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetAllStocksQueryHandler(IStockRepository stockRepository, IMapper mapper, IMemoryCache cache)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<StockDTO>> Handle(GetAllStocksQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = "GetAllStocks";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<StockDTO> stocksDto))
            {
                return stocksDto;
            }

            var stocks = await _stockRepository.GetPagedStocksAsync(request.Page, request.Limit);
            stocksDto = _mapper.Map<IEnumerable<StockDTO>>(stocks);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                SlidingExpiration = TimeSpan.FromSeconds(15)
            };

            _cache.Set(cacheKey, stocksDto, cacheOptions);

            return stocksDto;
        }
    }
}
