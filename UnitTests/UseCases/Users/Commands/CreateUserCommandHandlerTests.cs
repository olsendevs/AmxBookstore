using Xunit;
using Moq;
using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Users.Commands.CreateUser;
using Domain.Entities.Users;
using System;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Application.DTOs;
using Microsoft.AspNetCore.Http;
using AmxBookstore.Domain.Entities.Users.Enum;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenUserRoleIsClient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDTO { Name = "Test User", Email = "test@example.com", Role = UserRoles.Client.ToString() };
        var user = new User(userDto.Name, userDto.Email, "password", UserRoles.Client, userId);
        var command = new CreateUserCommand { User = userDto, UserRole = "Seller" };

        _mapperMock.Setup(m => m.Map<User>(userDto)).Returns(user);
        _userRepositoryMock.Setup(r => r.AddAsync(user, "password")).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(userId, result);
        _mapperMock.Verify(m => m.Map<User>(userDto), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSellerTriesToCreateNonClientUser()
    {
        // Arrange
        var userDto = new UserDTO { Name = "Test User", Email = "test@example.com", Role = UserRoles.Admin.ToString() };
        var user = new User(userDto.Name, userDto.Email, "password", UserRoles.Seller);
        var command = new CreateUserCommand { User = userDto, UserRole = "Seller" };

        _mapperMock.Setup(m => m.Map<User>(userDto)).Returns(user);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _handler.Handle(command, CancellationToken.None));
        _mapperMock.Verify(m => m.Map<User>(userDto), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), "password"), Times.Never);
    }
}
