using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Domain.Entities.Users;
using AmxBookstore.Application.Models;
using AmxBookstore.Application.Services;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using MockQueryable.Moq;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Entities.Users.Enum;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger>();
        _jwtSettings = Options.Create(new JwtSettings
        {
            Key = "389423IUOHIU345HIUREOIJQWAEOIJ3O45IJ34IO56J34OI5J32O4I5JOIJ345",
            Issuer = "YourIssuer",
            Audience = "YourAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        });

        _authService = new AuthService(
            _userManagerMock.Object,
            _configurationMock.Object,
            _loggerMock.Object,
            _jwtSettings
        );
    }

    private void SeedDatabase()
    {
        var users = new List<User>
        {
            new User("User 1", "user1@example.com", "Password1!", UserRoles.Client) { RefreshToken = "valid-refresh-token", RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1) },
            new User("User 2", "user2@example.com", "Password2!", UserRoles.Seller)
        }.AsQueryable().BuildMock();

        _userManagerMock.Setup(um => um.Users).Returns(users);
        _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                        .ReturnsAsync((string email) => users.FirstOrDefault(u => u.Email == email));
        _userManagerMock.Setup(um => um.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(true);
        _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                        .ReturnsAsync((string id) => users.FirstOrDefault(u => u.Id.ToString() == id));
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<User>()))
                        .ReturnsAsync(new List<string> { UserRoles.Client.ToString() });
        _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                        .ReturnsAsync(IdentityResult.Success);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        SeedDatabase();

        var loginDto = new LoginDTO { Email = "user1@example.com", Password = "Password1!" };

        var token = await _authService.LoginAsync(loginDto);

        Assert.NotNull(token);
        Assert.False(string.IsNullOrEmpty(token.AccessToken));
        Assert.False(string.IsNullOrEmpty(token.RefreshToken));
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnToken_WhenRefreshTokenIsValid()
    {
        SeedDatabase();

        var token = await _authService.RefreshTokenAsync("valid-refresh-token");

        Assert.NotNull(token);
        Assert.False(string.IsNullOrEmpty(token.AccessToken));
        Assert.False(string.IsNullOrEmpty(token.RefreshToken));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenCredentialsAreInvalid()
    {
        SeedDatabase();

        var loginDto = new LoginDTO { Email = "invalid@example.com", Password = "InvalidPassword!" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowException_WhenRefreshTokenIsInvalid()
    {
        SeedDatabase();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshTokenAsync("invalid-refresh-token"));
    }
}
