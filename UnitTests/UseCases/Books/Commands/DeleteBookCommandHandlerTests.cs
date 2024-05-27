using Xunit;
using Moq;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Books.Commands.DeleteBook;
using Domain.Entities.Books;
using System.Threading;
using System.Threading.Tasks;
using System;
using MediatR;

public class DeleteBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly DeleteBookCommandHandler _handler;

    public DeleteBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _handler = new DeleteBookCommandHandler(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBook_WhenBookExists()
    {
        var bookId = Guid.NewGuid();
        var book = new Book("Test Title", "Test Description", 123, "Test Author", 19.99m, bookId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(r => r.DeleteAsync(bookId)).Returns(Task.CompletedTask);

        var command = new DeleteBookCommand { Id = bookId };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        _bookRepositoryMock.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(r => r.DeleteAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenBookDoesNotExist()
    {
        var bookId = Guid.NewGuid();

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync((Book)null);

        var command = new DeleteBookCommand { Id = bookId };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _bookRepositoryMock.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
