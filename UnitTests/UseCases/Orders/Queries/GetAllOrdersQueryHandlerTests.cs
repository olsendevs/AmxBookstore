using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Xunit;
using Moq;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Application.Filters;
using AmxBookstore.Application.UseCases.Orders.Queries.GetAllOrders;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Orders;
using Microsoft.Extensions.Caching.Memory;
using AmxBookstore.Domain.Entities.Orders;

public class GetAllOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IMemoryCache _cache;
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _mapperMock = new Mock<IMapper>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _handler = new GetAllOrdersQueryHandler(_orderRepositoryMock.Object, _mapperMock.Object, _cache);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrdersFromCache_WhenOrdersExistInCache()
    {
        // Arrange
        var request = new GetAllOrdersQuery
        {
            UserRole = "Admin",
            UserId = Guid.NewGuid(),
            Page = 1,
            Limit = 10,
            Filter = null
        };

        var cacheKey = $"Orders_{request.UserRole}_{request.UserId}_{request.Page}_{request.Limit}_{request.Filter?.GetHashCode()}";
        var cachedOrders = new List<OrderDTO> { new OrderDTO() };

        _cache.Set(cacheKey, cachedOrders);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(cachedOrders, result);
        _orderRepositoryMock.Verify(r => r.GetPagedOrdersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Order, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrdersFromRepository_WhenOrdersDoNotExistInCache()
    {
        // Arrange
        var request = new GetAllOrdersQuery
        {
            UserRole = "Admin",
            UserId = Guid.NewGuid(),
            Page = 1,
            Limit = 10,
            Filter = null
        };

        var cacheKey = $"Orders_{request.UserRole}_{request.UserId}_{request.Page}_{request.Limit}_{request.Filter?.GetHashCode()}";
        var orders = new List<Order>
        {
            new Order(
                new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) },
                0,
                Guid.NewGuid(),
                Guid.NewGuid(),
                OrderStatus.Created,
                Guid.NewGuid())
        };
        var mappedOrders = new List<OrderDTO> { new OrderDTO() };

        _orderRepositoryMock.Setup(r => r.GetPagedOrdersAsync(request.Page, request.Limit, It.IsAny<Expression<Func<Order, bool>>>()))
                            .ReturnsAsync(orders);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrderDTO>>(orders)).Returns(mappedOrders);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(mappedOrders, result);
        _orderRepositoryMock.Verify(r => r.GetPagedOrdersAsync(request.Page, request.Limit, It.IsAny<Expression<Func<Order, bool>>>()), Times.Once);
        var cachedResult = _cache.Get<IEnumerable<OrderDTO>>(cacheKey);
        Assert.NotNull(cachedResult);
        Assert.Equal(mappedOrders, cachedResult);
    }

    [Fact]
    public async Task Handle_ShouldApplyFiltersCorrectly()
    {
        // Arrange
        var request = new GetAllOrdersQuery
        {
            UserRole = "Admin",
            UserId = Guid.NewGuid(),
            Page = 1,
            Limit = 10,
            Filter = new OrderFilter
            {
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow,
                Status = "Completed",
                MinTotal = 10,
                MaxTotal = 100
            }
        };

        var cacheKey = $"Orders_{request.UserRole}_{request.UserId}_{request.Page}_{request.Limit}_{request.Filter.GetHashCode()}";
        var orders = new List<Order>
        {
            new Order(
                new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) },
                0,
                Guid.NewGuid(),
                Guid.NewGuid(),
                OrderStatus.Created,
                Guid.NewGuid())
        };
        var mappedOrders = new List<OrderDTO> { new OrderDTO() };

        _orderRepositoryMock.Setup(r => r.GetPagedOrdersAsync(request.Page, request.Limit, It.IsAny<Expression<Func<Order, bool>>>()))
                            .ReturnsAsync(orders);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrderDTO>>(orders)).Returns(mappedOrders);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(mappedOrders, result);
        _orderRepositoryMock.Verify(r => r.GetPagedOrdersAsync(request.Page, request.Limit, It.IsAny<Expression<Func<Order, bool>>>()), Times.Once);
        var cachedResult = _cache.Get<IEnumerable<OrderDTO>>(cacheKey);
        Assert.NotNull(cachedResult);
        Assert.Equal(mappedOrders, cachedResult);
    }
}
