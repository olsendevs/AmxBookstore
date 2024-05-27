using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Books.Commands.UpdateBook;
using AmxBookstore.Application.DTOs;
using Domain.Entities.Books;
using System.Threading;
using System.Threading.Tasks;
using System;
using MediatR;

public class UpdateBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateBookCommandHandler _handler;

    public UpdateBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new UpdateBookCommandHandler(_bookRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBook_WhenBookExists()
    {
        var bookId = Guid.NewGuid();
        var bookDto = new BookDTO
        {
            Id = bookId,
            Title = "Updated Title",
            Description = "Updated Description",
            Pages = 200,
            Author = "Updated Author",
            Price = 29.99m
        };
        var book = new Book("Original Title", "Original Description", 100, "Original Author", 19.99m, bookId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);
        _mapperMock.Setup(m => m.Map(bookDto, book)).Returns(book);
        _bookRepositoryMock.Setup(r => r.UpdateAsync(book)).Returns(Task.CompletedTask);

        var command = new UpdateBookCommand { Book = bookDto };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        _bookRepositoryMock.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(r => r.UpdateAsync(book), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenBookDoesNotExist()
    {
        var bookId = Guid.NewGuid();
        var bookDto = new BookDTO
        {
            Id = bookId,
            Title = "Updated Title",
            Description = "Updated Description",
            Pages = 200,
            Author = "Updated Author",
            Price = 29.99m
        };

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync((Book)null);

        var command = new UpdateBookCommand { Book = bookDto };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _bookRepositoryMock.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
    }
}
