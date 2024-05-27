using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Books.Commands.CreateBook;
using AmxBookstore.Application.DTOs;
using Domain.Entities.Books;
using System.Threading;
using System.Threading.Tasks;
using System;

public class CreateBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateBookCommandHandler _handler;

    public CreateBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _stockRepositoryMock = new Mock<IStockRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateBookCommandHandler(_bookRepositoryMock.Object, _stockRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBookId_WhenBookIsCreatedSuccessfully()
    {
        var bookDto = new BookDTO
        {
            Title = "Test Title",
            Description = "Test Description",
            Pages = 123,
            Author = "Test Author",
            Price = 19.99m
        };

        var command = new CreateBookCommand { Book = bookDto };
        var bookId = Guid.NewGuid();

        var book = new Book(bookDto.Title, bookDto.Description, bookDto.Pages, bookDto.Author, bookDto.Price, bookId);

        _mapperMock.Setup(m => m.Map<Book>(command.Book)).Returns(book);
        _bookRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(bookId, result);
        _bookRepositoryMock.Verify(r => r.AddAsync(It.Is<Book>(b => b.Id == bookId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenBookCreationFails()
    {
        var bookDto = new BookDTO
        {
            Title = "Test Title",
            Description = "Test Description",
            Pages = 123,
            Author = "Test Author",
            Price = 19.99m
        };

        var command = new CreateBookCommand { Book = bookDto };

        _mapperMock.Setup(m => m.Map<Book>(command.Book)).Returns(It.IsAny<Book>());
        _bookRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Book>())).ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        _bookRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Once);
    }
}
