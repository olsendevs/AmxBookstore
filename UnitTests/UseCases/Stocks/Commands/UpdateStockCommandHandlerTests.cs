using Xunit;
using Moq;
using MediatR;
using AutoMapper;
using System;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Stocks.Commands.UpdateStock;
using Domain.Entities.Stocks;
using AmxBookstore.Application.DTOs;

public class UpdateStockCommandHandlerTests
{
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateStockCommandHandler _handler;

    public UpdateStockCommandHandlerTests()
    {
        _stockRepositoryMock = new Mock<IStockRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new UpdateStockCommandHandler(_stockRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStock_WhenStockExists()
    {
        // Arrange
        var stockId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var existingStock = new Stock(bookId, 10, stockId);
        var updatedStockDto = new StockDTO { Id = stockId, BookId = bookId, Quantity = 20 };
        var updatedStock = new Stock(bookId, 20, stockId);

        _stockRepositoryMock.Setup(r => r.GetByIdAsync(stockId)).ReturnsAsync(existingStock);
        _mapperMock.Setup(m => m.Map(updatedStockDto, existingStock)).Returns(updatedStock);

        var command = new UpdateStockCommand { Id = updatedStockDto.Id, Quantity = updatedStock.Quantity };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenStockNotFound()
    {
        // Arrange
        var stockId = Guid.NewGuid();
        var updatedStockDto = new StockDTO { Id = stockId, BookId = Guid.NewGuid(), Quantity = 20 };

        _stockRepositoryMock.Setup(r => r.GetByIdAsync(stockId)).ReturnsAsync((Stock)null);

        var command = new UpdateStockCommand { Id = updatedStockDto.Id, Quantity = updatedStockDto.Quantity };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _stockRepositoryMock.Verify(r => r.GetByIdAsync(stockId), Times.Once);
        _stockRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Stock>()), Times.Never);
    }
}
