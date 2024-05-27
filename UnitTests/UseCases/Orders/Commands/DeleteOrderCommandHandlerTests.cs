using Xunit;
using Moq;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Orders.Commands.DeleteOrder;
using Domain.Entities.Orders;
using Microsoft.AspNetCore.Http;
using AmxBookstore.Domain.Entities.Orders;
using Domain.Entities.Users;

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new DeleteOrderCommandHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOrder_WhenAdminRole()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(
         new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) },
         0,
         Guid.NewGuid(),
         Guid.NewGuid(),
         OrderStatus.Created,
         orderId
     );
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        var command = new DeleteOrderCommand { Id = orderId, UserRole = "Admin" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _orderRepositoryMock.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(r => r.DeleteAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOrder_WhenSellerRole()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var order = new Order(
              new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) },
              0,
              Guid.NewGuid(),
              Guid.NewGuid(),
              OrderStatus.Created,
              orderId
          );
        _orderRepositoryMock.Setup(r => r.GetByIdAndSellerIdAsync(orderId, sellerId)).ReturnsAsync(order);
        var command = new DeleteOrderCommand { Id = orderId, UserRole = "Seller", UserId = sellerId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _orderRepositoryMock.Verify(r => r.GetByIdAndSellerIdAsync(orderId, sellerId), Times.Once);
        _orderRepositoryMock.Verify(r => r.DeleteAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order)null);
        var command = new DeleteOrderCommand { Id = orderId, UserRole = "Admin" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _orderRepositoryMock.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSellerOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAndSellerIdAsync(orderId, sellerId)).ReturnsAsync((Order)null);
        var command = new DeleteOrderCommand { Id = orderId, UserRole = "Seller", UserId = sellerId };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _orderRepositoryMock.Verify(r => r.GetByIdAndSellerIdAsync(orderId, sellerId), Times.Once);
        _orderRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
