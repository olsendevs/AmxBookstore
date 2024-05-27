using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Stocks.Commands.CreateStock
{
    public class CreateStockCommand : IRequest<Guid>
    {
        public StockDTO Stock { get; set; }
    }
}
