using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Application.UseCases.Users.Queries.GetUserById;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Microsoft.Extensions.Logging;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserFromCache_WhenUserExistsInCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDTO { Id = userId, Name = "Test User" };
        var cacheKey = $"User_{userId}";

        object cachedValue = userDto;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedValue)).Returns(true);

        var query = new GetUserByIdQuery { Id = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(userDto, result);
        _cacheMock.Verify(cache => cache.TryGetValue(cacheKey, out cachedValue), Times.Once);
        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserFromRepository_WhenUserDoesNotExistInCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("Test User", "test@test.com", "Password123", AmxBookstore.Domain.Entities.Users.Enum.UserRoles.Client);
        var userDto = new UserDTO { Id = userId, Name = "Test User" };
        var cacheKey = $"User_{userId}";

        object cachedValue = null;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedValue)).Returns(false);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _mapperMock.Setup(mapper => mapper.Map<UserDTO>(user)).Returns(userDto);
        _cacheMock.Setup(cache => cache.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        var query = new GetUserByIdQuery { Id = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(userDto, result);
        _cacheMock.Verify(cache => cache.TryGetValue(cacheKey, out cachedValue), Times.Once);
        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<UserDTO>(user), Times.Once);
        _cacheMock.Verify(cache => cache.CreateEntry(cacheKey), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cacheKey = $"User_{userId}";

        object cachedValue = null;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedValue)).Returns(false);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

        var query = new GetUserByIdQuery { Id = userId };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        _cacheMock.Verify(cache => cache.TryGetValue(cacheKey, out cachedValue), Times.Once);
        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<UserDTO>(It.IsAny<User>()), Times.Never);
        _cacheMock.Verify(cache => cache.CreateEntry(It.IsAny<object>()), Times.Never);
    }
}
