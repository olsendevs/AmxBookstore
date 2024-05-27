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
using Domain.Entities.Books;
using AmxBookstore.Infrastructure.Identity;

public class BooksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public BooksControllerTests(WebApplicationFactory<Program> factory)
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

        // Seed books for GetAll test
        appDb.Books.AddRange(
            new Book("Book 1", "Description 1", 100, "Author 1", 19.99m),
            new Book("Book 2", "Description 2", 200, "Author 2", 29.99m)
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
    public async Task Create_ShouldReturnBookId_WhenBookIsCreated()
    {
        await AuthenticateAsync();

        // Arrange
        var bookDto = new BookDTO { Title = "Test Book", Author = "Test Author", Description = "Test Description", Pages = 123, Price = 49.99m };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Books", bookDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenBookIsUpdated()
    {
        await AuthenticateAsync();

        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var book = new Book("Original Title", "Original Description", 150, "Original Author", 59.99m);
            db.Books.Add(book);
            await db.SaveChangesAsync();

            var updatedBookDto = new BookDTO { Id = book.Id, Title = "Updated Title", Author = "Updated Author", Description = "Updated Description", Pages = 150, Price = 59.99m };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/Books", updatedBookDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenBookIsDeleted()
    {
        await AuthenticateAsync();

        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var book = new Book("Book to Delete", "Description", 100, "Author", 19.99m);
            db.Books.Add(book);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/v1/Books/{book.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }
    }

    [Fact]
    public async Task GetById_ShouldReturnBook_WhenBookExists()
    {
        await AuthenticateAsync();

        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var book = new Book("Test Book", "Test Description", 123, "Test Author", 49.99m);
            db.Books.Add(book);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/v1/Books/{book.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<BookDTO>();
            Assert.NotNull(result);
            Assert.Equal(book.Id, result.Id);
            Assert.Equal(book.Title, result.Title);
        }
    }

    [Fact]
    public async Task GetAll_ShouldReturnBooks_WhenBooksExist()
    {
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/Books");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<BookDTO>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
