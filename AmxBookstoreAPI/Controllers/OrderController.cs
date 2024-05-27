using MediatR;
using Microsoft.AspNetCore.Mvc;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Application.UseCases.Orders.Commands.CreateOrder;
using AmxBookstore.Application.UseCases.Orders.Commands.UpdateOrder;
using AmxBookstore.Application.UseCases.Orders.Commands.DeleteOrder;
using AmxBookstore.Application.UseCases.Orders.Queries.GetOrderById;
using AmxBookstore.Application.UseCases.Orders.Queries.GetAllOrders;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AmxBookstore.Application.Filters;

namespace AmxBookstoreAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "Order")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Create(OrderDTO orderDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var command = new CreateOrderCommand { Order = orderDto, UserId = Guid.Parse(userId), UserRole = userRole };
            var orderId = await _mediator.Send(command);
            return Ok(orderId);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Update(OrderDTO orderDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var command = new UpdateOrderCommand { Order = orderDto, UserId = Guid.Parse(userId), UserRole = userRole };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var command = new DeleteOrderCommand { Id = id, UserId = Guid.Parse(userId), UserRole = userRole };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller,Client")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var query = new GetOrderByIdQuery { Id = id, UserId = Guid.Parse(userId), UserRole = userRole };
            var order = await _mediator.Send(query);
            return Ok(order);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller,Client")]
        public async Task<IActionResult> GetAll([FromQuery] OrderFilter filter, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var query = new GetAllOrdersQuery
            {
                UserId = Guid.Parse(userId),
                UserRole = userRole,
                Page = page,
                Limit = limit,
                Filter = filter
            };

            var orders = await _mediator.Send(query);
            return Ok(orders);

        }
    }
}
