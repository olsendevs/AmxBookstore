using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Application.UseCases.Stocks.Queries.GetAllStocks;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Stocks;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetAllStocksQueryHandlerTests
{
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly GetAllStocksQueryHandler _handler;

    public GetAllStocksQueryHandlerTests()
    {
        _stockRepositoryMock = new Mock<IStockRepository>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();
        _handler = new GetAllStocksQueryHandler(_stockRepositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnStocksFromCache_WhenStocksExistInCache()
    {
        // Arrange
        var cacheKey = "GetAllStocks";
        var cachedStocks = new List<StockDTO> { new StockDTO { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = 10 } };

        object cachedStocksObj = cachedStocks;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedStocksObj)).Returns(true);

        var query = new GetAllStocksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedStocks, result);
        _cacheMock.Verify(cache => cache.TryGetValue(cacheKey, out cachedStocksObj), Times.Once);
        _stockRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnStocksFromRepository_WhenStocksDoNotExistInCache()
    {
        // Arrange
        var cacheKey = "GetAllStocks";
        var stocks = new List<Stock> { new Stock(Guid.NewGuid(), 10, Guid.NewGuid()) };
        var stocksDto = new List<StockDTO> { new StockDTO { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = 10 } };

        object cachedStocksObj = null;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedStocksObj)).Returns(false);
        _stockRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(stocks);
        _mapperMock.Setup(m => m.Map<IEnumerable<StockDTO>>(stocks)).Returns(stocksDto);
        _cacheMock.Setup(cache => cache.CreateEntry(cacheKey)).Returns(Mock.Of<ICacheEntry>);

        var query = new GetAllStocksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }
}
