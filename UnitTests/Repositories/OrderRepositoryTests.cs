using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using AmxBookstore.Infrastructure.Persistence;
using AmxBookstore.Infrastructure.Repositories;
using Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AmxBookstore.Domain.Entities.Orders;

public class OrderRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public OrderRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDb")
            .Options;
    }

    private async Task SeedDatabase(AppDbContext context)
    {
        var orders = new List<Order>
        {
            new Order(new List<OrderItem> { new OrderItem(Guid.NewGuid(), 2) }, 40.00M, Guid.NewGuid(), Guid.NewGuid(), OrderStatus.Created),
            new Order(new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) }, 10.00M, Guid.NewGuid(), Guid.NewGuid(), OrderStatus.Created)
        };

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);
            var orderId = context.Orders.First().Id;

            var order = await repository.GetByIdAsync(orderId);

            Assert.NotNull(order);
            Assert.Equal(orderId, order.Id);
        }
    }

    [Fact]
    public async Task GetByIdAndSellerIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);
            var order = context.Orders.First();

            var retrievedOrder = await repository.GetByIdAndSellerIdAsync(order.Id, order.SellerId);

            Assert.NotNull(retrievedOrder);
            Assert.Equal(order.Id, retrievedOrder.Id);
            Assert.Equal(order.SellerId, retrievedOrder.SellerId);
        }
    }

    [Fact]
    public async Task GetByIdAndClientIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);
            var order = context.Orders.First();

            var retrievedOrder = await repository.GetByIdAndClientIdAsync(order.Id, order.ClientId);

            Assert.NotNull(retrievedOrder);
            Assert.Equal(order.Id, retrievedOrder.Id);
            Assert.Equal(order.ClientId, retrievedOrder.ClientId);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrders()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);

            var orders = await repository.GetAllAsync();

            Assert.NotNull(orders);
            Assert.NotEmpty(orders);
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddOrder()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            var repository = new OrderRepository(context);
            var newOrder = new Order(new List<OrderItem> { new OrderItem(Guid.NewGuid(), 1) }, 15.00M, Guid.NewGuid(), Guid.NewGuid(), OrderStatus.Created);

            await repository.AddAsync(newOrder);

            var order = await context.Orders.FindAsync(newOrder.Id);
            Assert.NotNull(order);
            Assert.Equal(newOrder.TotalAmount, order.TotalAmount);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOrder()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);
            var order = context.Orders.First();
            order.UpdateStatus(OrderStatus.Delivering);

            await repository.UpdateAsync(order);

            var updatedOrder = await context.Orders.FindAsync(order.Id);
            Assert.NotNull(updatedOrder);
            Assert.Equal(OrderStatus.Delivering, updatedOrder.Status);
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkOrderAsDeleted()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);
            var orderId = context.Orders.First().Id;

            await repository.DeleteAsync(orderId);

            var order = await context.Orders.FindAsync(orderId);
            Assert.True(order.Deleted);
        }
    }

    [Fact]
    public async Task GetPagedOrdersForClientAsync_ShouldReturnPagedOrders()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);
            var clientId = context.Orders.First().ClientId;

            var orders = await repository.GetPagedOrdersForClientAsync(1, 1, clientId);

            Assert.NotNull(orders);
            Assert.Single(orders);
        }
    }

    [Fact]
    public async Task GetPagedOrdersForSellerAsync_ShouldReturnPagedOrders()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);
            var sellerId = context.Orders.First().SellerId;

            var orders = await repository.GetPagedOrdersForSellerAsync(1, 1, sellerId);

            Assert.NotNull(orders);
            Assert.Single(orders);
        }
    }

    [Fact]
    public async Task GetPagedOrdersAsync_ShouldReturnPagedOrders()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new OrderRepository(context);

            var orders = await repository.GetPagedOrdersAsync(1, 1);

            Assert.NotNull(orders);
            Assert.Single(orders);
        }
    }
}
