using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using AmxBookstore.Application.DTOs;
using System.Net.Http.Headers;
using AmxBookstore.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Moq;
using MediatR;
using AmxBookstore.Application.UseCases.Stocks.Commands.CreateStock;
using AmxBookstore.Application.UseCases.Stocks.Queries.GetStockById;
using AmxBookstore.Application.UseCases.Stocks.Queries.GetAllStocks;
using AmxBookstore.Infrastructure.Identity;

public class StocksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public StocksControllerTests(WebApplicationFactory<Program> factory)
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
                mockMediator.Setup(m => m.Send(It.IsAny<CreateStockCommand>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(Guid.NewGuid());


                mockMediator.Setup(m => m.Send(It.IsAny<GetStockByIdQuery>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new StockDTO());

                mockMediator.Setup(m => m.Send(It.IsAny<GetAllStocksQuery>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<StockDTO>());

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
        await userDb.SaveChangesAsync();
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
    public async Task Create_ShouldReturnStockId_WhenStockIsCreated()
    {
        await AuthenticateAsync();

        // Arrange
        var stockDto = new StockDTO
        {
            // Initialize properties for StockDTO
            Id = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Quantity = 20,
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Stocks", stockDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenStockIsUpdated()
    {
        await AuthenticateAsync();

        // Arrange
        var stockDto = new StockDTO
        {
            // Initialize properties for StockDTO
            Id = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Quantity = 20,
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/Stocks/{stockDto.Id}", 1);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenStockIsDeleted()
    {
        await AuthenticateAsync();

        // Arrange
        var stockId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/Stocks/{stockId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnStock_WhenStockExists()
    {
        await AuthenticateAsync();

        // Arrange
        var stockId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/Stocks/{stockId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<StockDTO>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAll_ShouldReturnStocks_WhenStocksExist()
    {
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/Stocks");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<StockDTO>>();
        Assert.NotNull(result);
    }
}
