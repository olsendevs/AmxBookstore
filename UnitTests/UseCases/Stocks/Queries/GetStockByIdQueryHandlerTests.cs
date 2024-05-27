using Xunit;
using Moq;
using MediatR;
using AutoMapper;
using AmxBookstore.Application.UseCases.Stocks.Queries.GetStockById;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Stocks;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

public class GetStockByIdQueryHandlerTests
{
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly GetStockByIdQueryHandler _handler;

    public GetStockByIdQueryHandlerTests()
    {
        _stockRepositoryMock = new Mock<IStockRepository>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();
        _handler = new GetStockByIdQueryHandler(_stockRepositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnStockFromCache_WhenStockExistsInCache()
    {
        // Arrange
        var stockId = Guid.NewGuid();
        var stockDto = new StockDTO { Id = stockId, BookId = Guid.NewGuid(), Quantity = 10 };
        var cacheKey = $"Stock_{stockId}";

        object cachedStock = stockDto;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedStock)).Returns(true);

        var query = new GetStockByIdQuery { Id = stockId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(stockDto.Quantity, result.Quantity);
        _cacheMock.Verify(cache => cache.TryGetValue(cacheKey, out cachedStock), Times.Once);
        _stockRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnStockFromRepository_WhenStockDoesNotExistInCache()
    {
        // Arrange
        var stockId = Guid.NewGuid();
        var stock = new Stock(Guid.NewGuid(), 10, stockId);
        var stockDto = new StockDTO { Id = stockId, BookId = stock.BookId, Quantity = 10 };
        var cacheKey = $"Stock_{stockId}";

        object cachedStock = null;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedStock)).Returns(false);
        _stockRepositoryMock.Setup(repo => repo.GetByIdAsync(stockId)).ReturnsAsync(stock);
        _mapperMock.Setup(mapper => mapper.Map<StockDTO>(stock)).Returns(stockDto);
        _cacheMock.Setup(cache => cache.CreateEntry(cacheKey)).Returns(Mock.Of<ICacheEntry>);

        var query = new GetStockByIdQuery { Id = stockId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(stockDto.Quantity, result.Quantity);
        _cacheMock.Verify(cache => cache.TryGetValue(cacheKey, out cachedStock), Times.Once);
        _stockRepositoryMock.Verify(repo => repo.GetByIdAsync(stockId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<StockDTO>(stock), Times.Once);
        _cacheMock.Verify(cache => cache.CreateEntry(cacheKey), Times.Once);
    }
}
