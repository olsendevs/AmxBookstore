using Xunit;
using Moq;
using MediatR;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Users.Commands.DeleteUser;
using Domain.Entities.Users;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new DeleteUserCommandHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("Test User", "test@example.com", "password", AmxBookstore.Domain.Entities.Users.Enum.UserRoles.Client, userId);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        var command = new DeleteUserCommand { Id = userId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);
        var command = new DeleteUserCommand { Id = userId };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
