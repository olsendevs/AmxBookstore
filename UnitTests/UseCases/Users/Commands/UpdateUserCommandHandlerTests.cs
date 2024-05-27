using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Application.UseCases.Users.Commands.UpdateUser;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Entities.Users.Enum;
using MediatR;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _mapperMock.Object);
    }


    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDTO { Id = userId, Name = "Updated User", Email = "updated@test.com" };
        var updateUserCommand = new UpdateUserCommand { User = userDto, UserRole = "Seller" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(updateUserCommand, CancellationToken.None));
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadHttpRequestException_WhenSellerTriesToUpdateNonClient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("Test Admin", "admin@test.com", "Password123", UserRoles.Admin, userId);
        var userDto = new UserDTO { Id = userId, Name = "Updated Admin", Email = "updated@test.com" };
        var updateUserCommand = new UpdateUserCommand { User = userDto, UserRole = "Seller" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _handler.Handle(updateUserCommand, CancellationToken.None));
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
