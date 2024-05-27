using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Application.UseCases.Orders.Queries.GetOrderById;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Orders;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Domain.Entities.Orders;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IMemoryCache _cache;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _mapperMock = new Mock<IMapper>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _handler = new GetOrderByIdQueryHandler(_orderRepositoryMock.Object, _mapperMock.Object, _cache);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderFromCache_WhenOrderExistsInCache()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderDto = new OrderDTO { Id = orderId, Status = "Created" };
        var cacheKey = $"Order_Admin_{userId}_{orderId}";

        _cache.Set(cacheKey, orderDto);

        var query = new GetOrderByIdQuery { Id = orderId, UserId = userId, UserRole = "Admin" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderDto.Status, result.Status);
        _orderRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderFromRepository_WhenOrderDoesNotExistInCache()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order(
            new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) },
            100,
            Guid.NewGuid(),
            userId,
            OrderStatus.Created,
            Guid.NewGuid());
        var orderDto = new OrderDTO { Id = orderId, Status = "Created" };
        var cacheKey = $"Order_Admin_{userId}_{orderId}";

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mapperMock.Setup(mapper => mapper.Map<OrderDTO>(order)).Returns(orderDto);

        var query = new GetOrderByIdQuery { Id = orderId, UserId = userId, UserRole = "Admin" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderDto.Status, result.Status);
        _orderRepositoryMock.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<OrderDTO>(order), Times.Once);
        var cachedResult = _cache.Get<OrderDTO>(cacheKey);
        Assert.NotNull(cachedResult);
        Assert.Equal(orderDto.Status, cachedResult.Status);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetOrderByIdQuery { Id = orderId, UserId = userId, UserRole = "Admin" };

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId)).ReturnsAsync((Order)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        _orderRepositoryMock.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderFromRepository_ForClientRole()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order(
            new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) },
            100,
            Guid.NewGuid(),
            userId,
            OrderStatus.Created,
            Guid.NewGuid());
        var orderDto = new OrderDTO { Id = orderId, Status = "Created" };
        var cacheKey = $"Order_Client_{userId}_{orderId}";

        _orderRepositoryMock.Setup(repo => repo.GetByIdAndClientIdAsync(orderId, userId)).ReturnsAsync(order);
        _mapperMock.Setup(mapper => mapper.Map<OrderDTO>(order)).Returns(orderDto);

        var query = new GetOrderByIdQuery { Id = orderId, UserId = userId, UserRole = "Client" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderDto.Status, result.Status);
        _orderRepositoryMock.Verify(repo => repo.GetByIdAndClientIdAsync(orderId, userId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<OrderDTO>(order), Times.Once);
        var cachedResult = _cache.Get<OrderDTO>(cacheKey);
        Assert.NotNull(cachedResult);
        Assert.Equal(orderDto.Status, cachedResult.Status);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderFromRepository_ForSellerRole()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order(
            new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) },
            100,
            userId,
            Guid.NewGuid(),
            OrderStatus.Created,
            Guid.NewGuid());
        var orderDto = new OrderDTO { Id = orderId, Status = "Created" };
        var cacheKey = $"Order_Seller_{userId}_{orderId}";

        _orderRepositoryMock.Setup(repo => repo.GetByIdAndSellerIdAsync(orderId, userId)).ReturnsAsync(order);
        _mapperMock.Setup(mapper => mapper.Map<OrderDTO>(order)).Returns(orderDto);

        var query = new GetOrderByIdQuery { Id = orderId, UserId = userId, UserRole = "Seller" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderDto.Status, result.Status);
        _orderRepositoryMock.Verify(repo => repo.GetByIdAndSellerIdAsync(orderId, userId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<OrderDTO>(order), Times.Once);
        var cachedResult = _cache.Get<OrderDTO>(cacheKey);
        Assert.NotNull(cachedResult);
        Assert.Equal(orderDto.Status, cachedResult.Status);
    }
}
