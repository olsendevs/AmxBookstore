using Xunit;
using Moq;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Stocks.Commands.DeleteStock;
using Domain.Entities.Stocks;

public class DeleteStockCommandHandlerTests
{
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly DeleteStockCommandHandler _handler;

    public DeleteStockCommandHandlerTests()
    {
        _stockRepositoryMock = new Mock<IStockRepository>();
        _handler = new DeleteStockCommandHandler(_stockRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteStock_WhenStockExists()
    {
        // Arrange
        var stockId = Guid.NewGuid();
        var stock = new Stock(stockId, 10, stockId);
        _stockRepositoryMock.Setup(r => r.GetByIdAsync(stockId)).ReturnsAsync(stock);
        var command = new DeleteStockCommand { Id = stockId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _stockRepositoryMock.Verify(r => r.GetByIdAsync(stockId), Times.Once);
        _stockRepositoryMock.Verify(r => r.DeleteAsync(stockId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenStockNotFound()
    {
        // Arrange
        var stockId = Guid.NewGuid();
        _stockRepositoryMock.Setup(r => r.GetByIdAsync(stockId)).ReturnsAsync((Stock)null);
        var command = new DeleteStockCommand { Id = stockId };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _stockRepositoryMock.Verify(r => r.GetByIdAsync(stockId), Times.Once);
        _stockRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
