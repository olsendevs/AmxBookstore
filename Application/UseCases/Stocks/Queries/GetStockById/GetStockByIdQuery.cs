using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Stocks.Queries.GetStockById
{
    public class GetStockByIdQuery : IRequest<StockDTO>
    {
        public Guid Id { get; set; }
    }
}
