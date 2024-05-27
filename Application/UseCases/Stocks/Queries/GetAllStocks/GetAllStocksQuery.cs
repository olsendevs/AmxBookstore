using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Stocks.Queries.GetAllStocks
{
    public class GetAllStocksQuery : IRequest<IEnumerable<StockDTO>>
    {
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = 10;
    }
}
