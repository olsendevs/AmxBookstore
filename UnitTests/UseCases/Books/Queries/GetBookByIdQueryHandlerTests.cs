using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Application.UseCases.Books.Queries.GetBookById;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Books;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

public class GetBookByIdQueryHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly GetBookByIdQueryHandler _handler;

    public GetBookByIdQueryHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();
        _handler = new GetBookByIdQueryHandler(_bookRepositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBookFromCache_WhenBookExistsInCache()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var bookDto = new BookDTO { Id = bookId, Title = "Test Book" };
        var cacheKey = $"Book_{bookId}";

        object cachedBook = bookDto;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedBook)).Returns(true);

        var query = new GetBookByIdQuery { Id = bookId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookDto.Title, result.Title);
        _cacheMock.Verify(cache => cache.TryGetValue(cacheKey, out cachedBook), Times.Once);
        _bookRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnBookFromRepository_WhenBookDoesNotExistInCache()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book("Test Title", "Test Description", 100, "Test Author", 19.99m, bookId);
        var bookDto = new BookDTO { Id = bookId, Title = "Test Book" };
        var cacheKey = $"Book_{bookId}";

        object cachedBook = null;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedBook)).Returns(false);
        _bookRepositoryMock.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(book);
        _mapperMock.Setup(mapper => mapper.Map<BookDTO>(book)).Returns(bookDto);
        _cacheMock.Setup(cache => cache.CreateEntry(cacheKey)).Returns(Mock.Of<ICacheEntry>);

        var query = new GetBookByIdQuery { Id = bookId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookDto.Title, result.Title);
        _cacheMock.Verify(cache => cache.TryGetValue(cacheKey, out cachedBook), Times.Once);
        _bookRepositoryMock.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<BookDTO>(book), Times.Once);
        _cacheMock.Verify(cache => cache.CreateEntry(cacheKey), Times.Once);
    }
}
