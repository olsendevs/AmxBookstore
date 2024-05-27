using Xunit;
using Moq;
using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Stocks.Commands.CreateStock;
using Domain.Entities.Stocks;
using Domain.Entities.Books;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Application.DTOs;

public class CreateStockCommandHandlerTests
{
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateStockCommandHandler _handler;

    public CreateStockCommandHandlerTests()
    {
        _stockRepositoryMock = new Mock<IStockRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateStockCommandHandler(_stockRepositoryMock.Object, _bookRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateStock_WhenBookExists()
    {
        // Arrange
        var stockId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var stock = new Stock(bookId, 10, stockId);
        var createStockCommand = new CreateStockCommand { Stock = new StockDTO { BookId = bookId, Quantity = 10 } };

        _mapperMock.Setup(m => m.Map<Stock>(createStockCommand.Stock)).Returns(stock);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(new Book("Title", "Description", 100, "Author", 10.99m, bookId));
        _stockRepositoryMock.Setup(r => r.AddAsync(stock)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(createStockCommand, CancellationToken.None);

        // Assert
        Assert.Equal(stockId, result);
        _bookRepositoryMock.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _stockRepositoryMock.Verify(r => r.AddAsync(stock), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var createStockCommand = new CreateStockCommand { Stock = new StockDTO { BookId = bookId, Quantity = 10 } };

        _mapperMock.Setup(m => m.Map<Stock>(createStockCommand.Stock)).Returns(new Stock(bookId, 10));
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync((Book)null);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _handler.Handle(createStockCommand, CancellationToken.None));
        _bookRepositoryMock.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _stockRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Stock>()), Times.Never);
    }
}
