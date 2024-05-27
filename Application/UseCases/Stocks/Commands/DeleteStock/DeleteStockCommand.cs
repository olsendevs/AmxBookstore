using MediatR;

namespace AmxBookstore.Application.UseCases.Stocks.Commands.DeleteStock
{
    public class DeleteStockCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
    }
}
