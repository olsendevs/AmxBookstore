using MediatR;
using Microsoft.AspNetCore.Mvc;
using AmxBookstore.Application.UseCases.Stocks.Commands.CreateStock;
using AmxBookstore.Application.UseCases.Stocks.Commands.UpdateStock;
using AmxBookstore.Application.UseCases.Stocks.Commands.DeleteStock;
using AmxBookstore.Application.UseCases.Stocks.Queries.GetStockById;
using AmxBookstore.Application.UseCases.Stocks.Queries.GetAllStocks;
using AmxBookstore.Application.DTOs;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using AmxBookstore.Application.UseCases.Stocks.Queries.GetStockByProductId;

namespace AmxBookstore.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "Stock")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StocksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(StockDTO stockDto)
        {
            var command = new CreateStockCommand { Stock = stockDto };
            var stockId = await _mediator.Send(command);
            return Ok(stockId);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] int quantity)
        {
            var command = new UpdateStockCommand { Id = id, Quantity = quantity };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteStockCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetStockByIdQuery { Id = id };
            var stock = await _mediator.Send(query);
            return Ok(stock);
        }

        [HttpGet("product/{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetByProductId(Guid id)
        {
            var query = new GetStockByProductIdQuery { Id = id };
            var stock = await _mediator.Send(query);
            return Ok(stock);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var query = new GetAllStocksQuery
                {
                    Page = page,
                    Limit = limit,
                }; 
            var stocks = await _mediator.Send(query);
            return Ok(stocks);
        }
    }
}
