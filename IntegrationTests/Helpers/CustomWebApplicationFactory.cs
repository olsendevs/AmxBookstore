using System.Net.Http.Json;
using System.Net.Http;
using AmxBookstore.Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AmxBookstore.Application.Interfaces;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Infrastructure.Persistence;
using AmxBookstore.Infrastructure.Repositories;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using MediatR;
using System;
using System.IO;
using System.Threading.Tasks;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public Mock<IAuthService> AuthServiceMock { get; }
    public Mock<IMediator> MediatorMock { get; }

    public CustomWebApplicationFactory()
    {
        AuthServiceMock = new Mock<IAuthService>();
        MediatorMock = new Mock<IMediator>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            services.AddScoped<IUserRepository, UserRepository>();

            // Replace the IAuthService with the mock
            services.AddSingleton(AuthServiceMock.Object);

            // Replace IMediator with the mock
            services.AddSingleton(MediatorMock.Object);

            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            using (var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>())
            {
                try
                {
                    appContext.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    // Log errors
                }
            }
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.json");

            config.AddJsonFile(configPath);
        });
    }

    public async Task<string> GetAuthTokenAsync()
    {
        var client = CreateClient();
        var loginDto = new LoginDTO { Email = "test@example.com", Password = "Password123!" };
        var response = await client.PostAsJsonAsync("/api/v1/Auth/login", loginDto);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Login Response: {content}");
        var token = await response.Content.ReadFromJsonAsync<TokenDTO>();
        return token?.AccessToken;
    }
}
