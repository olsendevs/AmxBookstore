using System.Net.Http.Json;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net.Http.Headers;
using AmxBookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using AmxBookstore.Infrastructure.Identity;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Mock IAuthService
                var mockAuthService = new Mock<IAuthService>();

                // Setup mock behavior for LoginAsync
                mockAuthService.Setup(service => service.LoginAsync(It.IsAny<LoginDTO>()))
                    .ReturnsAsync(new TokenDTO { AccessToken = "mock-access-token", RefreshToken = "mock-refresh-token" });

                // Setup mock behavior for RefreshTokenAsync
                mockAuthService.Setup(service => service.RefreshTokenAsync(It.IsAny<string>()))
                    .ReturnsAsync(new TokenDTO { AccessToken = "mock-access-token", RefreshToken = "mock-refresh-token" });

                services.AddSingleton(mockAuthService.Object);

                // Add in-memory database for UserDbContext
                services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryUserDbForTesting");
                });

                var sp = services.BuildServiceProvider();

                // Seed database
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var userDb = scopedServices.GetRequiredService<UserDbContext>();

                    userDb.Database.EnsureCreated();

                    SeedUserData(userDb, scopedServices).Wait();
                }
            });
        });
        _client = _factory.CreateClient();
    }

    private async Task SeedUserData(UserDbContext userDb, IServiceProvider scopedServices)
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

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var loginDto = new LoginDTO { Email = "admin@example.com", Password = "Admin123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenDTO>();
        Assert.NotNull(token);
        Assert.False(string.IsNullOrEmpty(token.AccessToken));
        Assert.False(string.IsNullOrEmpty(token.RefreshToken));
    }

    [Fact]
    public async Task Refresh_ShouldReturnNewToken_WhenRefreshTokenIsValid()
    {
        // Arrange
        var refreshToken = "mock-refresh-token";

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/refresh", refreshToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenDTO>();
        Assert.NotNull(token);
        Assert.False(string.IsNullOrEmpty(token.AccessToken));
        Assert.False(string.IsNullOrEmpty(token.RefreshToken));
    }
}
