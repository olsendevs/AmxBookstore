using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Application.UseCases.Users.Queries.GetAllUsers;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using AmxBookstore.Domain.Entities.Users.Enum;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();
        _handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUsersFromCache_WhenUsersExistInCache()
    {
        // Arrange
        var cacheKey = "GetAllUsers";
        var cachedUsers = new List<UserDTO> ();

        object cachedValue = cachedUsers;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedValue)).Returns(true);

        var query = new GetAllUsersQuery()
        {
            UserId = Guid.NewGuid(),
            UserRole = "Admin",
            Page = 0,
            Limit = 10,
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(cachedUsers, result);
        _userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUsersFromRepository_WhenUsersDoNotExistInCache()
    {
        // Arrange
        var cacheKey = "GetAllUsers";
        var users = new List<User> { new User("Test User", "test@test.com", "Password123", UserRoles.Client) };
        var usersDto = new List<UserDTO> ();

        object cachedValue = null;
        _cacheMock.Setup(cache => cache.TryGetValue(cacheKey, out cachedValue)).Returns(false);
        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<UserDTO>>(users)).Returns(usersDto);
        _cacheMock.Setup(cache => cache.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        var query = new GetAllUsersQuery()
        {
            UserId = Guid.NewGuid(),
            UserRole = "Admin",
            Page = 0,
            Limit = 10,
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(usersDto, result);
    }
}
