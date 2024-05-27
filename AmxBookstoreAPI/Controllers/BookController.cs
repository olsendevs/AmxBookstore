using MediatR;
using Microsoft.AspNetCore.Mvc;
using AmxBookstore.Application.UseCases.Books.Commands.CreateBook;
using AmxBookstore.Application.UseCases.Books.Commands.UpdateBook;
using AmxBookstore.Application.UseCases.Books.Commands.DeleteBook;
using AmxBookstore.Application.UseCases.Books.Queries.GetBookById;
using AmxBookstore.Application.UseCases.Books.Queries.GetAllBooks;
using AmxBookstore.Application.DTOs;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using AmxBookstore.Application.Filters;

namespace AmxBookstoreAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "Book")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BooksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(BookDTO bookDto)
        {
            var command = new CreateBookCommand { Book = bookDto };
            var bookId = await _mediator.Send(command);
            return Ok(bookId);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(BookDTO bookDto)
        {
            var command = new UpdateBookCommand { Book = bookDto };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteBookCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetBookByIdQuery { Id = id };
            var book = await _mediator.Send(query);
            return Ok(book);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetAll([FromQuery] BookFilter filter, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var query = new GetAllBooksQuery
            {
                Page = page,
                Limit = limit,
                Filter = filter
            };
            var books = await _mediator.Send(query);
            return Ok(books);
        }
    }
}
