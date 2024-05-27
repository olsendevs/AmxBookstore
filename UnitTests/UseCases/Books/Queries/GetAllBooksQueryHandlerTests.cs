using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Application.UseCases.Books.Queries.GetAllBooks;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Books;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Application.Filters;

public class GetAllBooksQueryHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllBooksQueryHandler _handler;

    public GetAllBooksQueryHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAllBooksQueryHandler(_bookRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBooks_WhenBooksExist()
    {
        // Arrange
        var filter = new BookFilter { Title = "Test" };
        var books = new List<Book>
        {
            new Book("Test Title", "Test Description", 100, "Test Author", 19.99m)
        };

        var bookDtos = new List<BookDTO>
        {
            new BookDTO { Title = "Test Title", Description = "Test Description", Pages = 100, Author = "Test Author", Price = 19.99m }
        };

        _bookRepositoryMock.Setup(repo => repo.GetPagedBooksAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Book, bool>>>()))
            .ReturnsAsync(books);
        _mapperMock.Setup(m => m.Map<IEnumerable<BookDTO>>(books)).Returns(bookDtos);

        var query = new GetAllBooksQuery { Filter = filter, Page = 1, Limit = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _bookRepositoryMock.Verify(repo => repo.GetPagedBooksAsync(1, 10, It.IsAny<Expression<Func<Book, bool>>>()), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<BookDTO>>(books), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoBooksExist()
    {
        // Arrange
        var filter = new BookFilter { Title = "NonExistingTitle" };
        var books = new List<Book>();
        var bookDtos = new List<BookDTO>();

        _bookRepositoryMock.Setup(repo => repo.GetPagedBooksAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Book, bool>>>()))
            .ReturnsAsync(books);
        _mapperMock.Setup(m => m.Map<IEnumerable<BookDTO>>(books)).Returns(bookDtos);

        var query = new GetAllBooksQuery { Filter = filter, Page = 1, Limit = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _bookRepositoryMock.Verify(repo => repo.GetPagedBooksAsync(1, 10, It.IsAny<Expression<Func<Book, bool>>>()), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<BookDTO>>(books), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldApplyFilter_WhenFilterIsProvided()
    {
        // Arrange
        var filter = new BookFilter { Title = "Test", Author = "Author" };
        var books = new List<Book>
        {
            new Book("Test Title", "Test Description", 100, "Test Author", 19.99m)
        };

        var bookDtos = new List<BookDTO>
        {
            new BookDTO { Title = "Test Title", Description = "Test Description", Pages = 100, Author = "Test Author", Price = 19.99m }
        };

        _bookRepositoryMock.Setup(repo => repo.GetPagedBooksAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Book, bool>>>()))
            .ReturnsAsync(books);
        _mapperMock.Setup(m => m.Map<IEnumerable<BookDTO>>(books)).Returns(bookDtos);

        var query = new GetAllBooksQuery { Filter = filter, Page = 1, Limit = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _bookRepositoryMock.Verify(repo => repo.GetPagedBooksAsync(1, 10, It.IsAny<Expression<Func<Book, bool>>>()), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<BookDTO>>(books), Times.Once);
    }
}
