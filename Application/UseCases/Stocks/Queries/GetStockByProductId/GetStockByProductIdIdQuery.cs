using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Stocks.Queries.GetStockByProductId
{
    public class GetStockByProductIdQuery : IRequest<StockDTO>
    {
        public Guid Id { get; set; }
    }
}
