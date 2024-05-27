using MediatR;
using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.UseCases.Stocks.Commands.UpdateStock
{
    public class UpdateStockCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
    }
}
