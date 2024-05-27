using System.Net.Http.Json;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using AmxBookstore.Application.DTOs;
using System.Net.Http.Headers;
using AmxBookstore.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using Moq;
using MediatR;
using AmxBookstore.Application.UseCases.Orders.Commands.CreateOrder;
using AmxBookstore.Application.UseCases.Orders.Commands.UpdateOrder;
using AmxBookstore.Application.UseCases.Orders.Commands.DeleteOrder;
using AmxBookstore.Application.UseCases.Orders.Queries.GetOrderById;
using AmxBookstore.Application.UseCases.Orders.Queries.GetAllOrders;
using AmxBookstore.Infrastructure.Identity;
using AmxBookstore.Domain.Entities.Orders;
using Domain.Entities.Orders;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContextOptions for AppDbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for AppDbContext
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Add in-memory database for UserDbContext
                services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryUserDbForTesting");
                });

                // Mock the IMediator
                var mockMediator = new Mock<IMediator>();

                // Setup mock behavior for each Mediator request
                mockMediator.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(Guid.NewGuid());


                mockMediator.Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new OrderDTO());

                mockMediator.Setup(m => m.Send(It.IsAny<GetAllOrdersQuery>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<OrderDTO>());

                services.AddSingleton(mockMediator.Object);

                var sp = services.BuildServiceProvider();

                // Seed databases
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var appDb = scopedServices.GetRequiredService<AppDbContext>();
                    var userDb = scopedServices.GetRequiredService<UserDbContext>();

                    appDb.Database.EnsureCreated();
                    userDb.Database.EnsureCreated();

                    SeedData(appDb, userDb, scopedServices).Wait();
                }
            });
        });
        _client = _factory.CreateClient();
    }

    private async Task SeedData(AppDbContext appDb, UserDbContext userDb, IServiceProvider scopedServices)
    {
        var userManager = scopedServices.GetRequiredService<UserManager<User>>();

        // Seed a user with known credentials
        var user = new User
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            Name = "Admin User"
        };
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, "Admin123!");

        userDb.Users.Add(user);

        // Seed any necessary data for tests
        var sellerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var orderItem = new OrderItem(Guid.NewGuid(), 1);

        appDb.Orders.AddRange(
            new Order(new List<OrderItem> { orderItem }, 100.0m, sellerId, clientId, OrderStatus.Created),
            new Order(new List<OrderItem> { orderItem }, 200.0m, sellerId, clientId, OrderStatus.Created)
        );

        await userDb.SaveChangesAsync();
        await appDb.SaveChangesAsync();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginDto = new LoginDTO { Email = "admin@example.com", Password = "Admin123!" };
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginDto);

        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenDTO>();
        return token?.AccessToken;
    }

    private async Task AuthenticateAsync()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Create_ShouldReturnOrderId_WhenOrderIsCreated()
    {
        await AuthenticateAsync();

        // Arrange
        var orderItemDto = new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 1 };
        var orderDto = new OrderDTO
        {
            Products = new List<OrderItemDTO> { orderItemDto },
            SellerId = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            Status = OrderStatus.Created.ToString()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Orders", orderDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenOrderIsUpdated()
    {
        await AuthenticateAsync();

        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sellerId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var orderItem = new OrderItem(Guid.NewGuid(), 1);
            var order = new Order(new List<OrderItem> { orderItem }, 100.0m, sellerId, clientId, OrderStatus.Created);
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var updatedOrderDto = new OrderDTO
            {
                Id = order.Id,
                Products = new List<OrderItemDTO> { new OrderItemDTO { ProductId = orderItem.ProductId, Quantity = 1} },
                SellerId = sellerId,
                ClientId = clientId,
                Status = OrderStatus.Created.ToString()
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/Orders", updatedOrderDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenOrderIsDeleted()
    {
        await AuthenticateAsync();

        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sellerId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var orderItem = new OrderItem(Guid.NewGuid(), 1);
            var order = new Order(new List<OrderItem> { orderItem }, 100.0m, sellerId, clientId, OrderStatus.Created);
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/v1/Orders/{order.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }
    }

    [Fact]
    public async Task GetById_ShouldReturnOrder_WhenOrderExists()
    {
        await AuthenticateAsync();

        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sellerId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var orderItem = new OrderItem(Guid.NewGuid(), 1);
            var order = new Order(new List<OrderItem> { orderItem }, 100.0m, sellerId, clientId, OrderStatus.Created);
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/v1/Orders/{order.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<OrderDTO>();
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task GetAll_ShouldReturnOrders_WhenOrdersExist()
    {
        await AuthenticateAsync();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sellerId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var orderItem = new OrderItem(Guid.NewGuid(), 1);
            var order = new Order(new List<OrderItem> { orderItem }, 100.0m, sellerId, clientId, OrderStatus.Created);
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/Orders");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<OrderDTO>>();
            Assert.NotNull(result);
        }
    }
}
