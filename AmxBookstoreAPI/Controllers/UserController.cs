using MediatR;
using Microsoft.AspNetCore.Mvc;
using AmxBookstore.Application.UseCases.Users.Commands.CreateUser;
using AmxBookstore.Application.UseCases.Users.Commands.UpdateUser;
using AmxBookstore.Application.UseCases.Users.Commands.DeleteUser;
using AmxBookstore.Application.UseCases.Users.Queries.GetUserById;
using AmxBookstore.Application.UseCases.Users.Queries.GetAllUsers;
using AmxBookstore.Application.DTOs;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AmxBookstore.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "User")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Create(UserDTO userDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var command = new CreateUserCommand { 
                User = userDto, 
                UserId = Guid.Parse(userId), 
                UserRole = userRole 
            };
            var createdUserId = await _mediator.Send(command);
            return Ok(userId);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Update(UserDTO userDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var command = new UpdateUserCommand { 
                User = userDto,
                UserId = Guid.Parse(userId),
                UserRole = userRole,
            };
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

            var command = new DeleteUserCommand { 
                Id = id,
                UserId = Guid.Parse(userId),
                UserRole = userRole,
            };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var query = new GetUserByIdQuery { 
                Id = id,
                UserId = Guid.Parse(userId),
                UserRole = userRole,
            };
            var user = await _mediator.Send(query);
            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList().FirstOrDefault();

            if (userId == null)
            {
                return Unauthorized("User not authorized.");
            }

            var query = new GetAllUsersQuery
                {
                    UserId = Guid.Parse(userId),
                    UserRole = userRole,
                    Page = page,
                    Limit = limit,
                };
            var users = await _mediator.Send(query);
            return Ok(users);
        }
    }
}
