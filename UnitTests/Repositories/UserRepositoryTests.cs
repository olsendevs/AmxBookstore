using Xunit;
using Moq;
using MockQueryable.Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Domain.Entities.Users;
using AmxBookstore.Application.Models;
using AmxBookstore.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmxBookstore.Domain.Entities.Users.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;

public class UserRepositoryTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;

    public UserRepositoryTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private async Task SeedDatabase(Mock<UserManager<User>> userManagerMock)
    {
        var users = new List<User>
        {
            new User("User 1", "user1@example.com", "Password1!", UserRoles.Client),
            new User("User 2", "user2@example.com", "Password2!", UserRoles.Seller)
        };

        var usersMock = users.AsQueryable().BuildMock();

        userManagerMock.Setup(um => um.Users).Returns(usersMock);
        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                       .ReturnsAsync((string id) => users.FirstOrDefault(u => u.Id.ToString() == id));
        userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((string email) => users.FirstOrDefault(u => u.Email == email));
        userManagerMock.Setup(um => um.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
                       .ReturnsAsync(true);
        userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                       .ReturnsAsync(IdentityResult.Success);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var userManager = _userManagerMock.Object;
        await SeedDatabase(_userManagerMock);
        var repository = new UserRepository(userManager);
        var userId = (await userManager.Users.FirstAsync()).Id;

        var user = await repository.GetByIdAsync(userId);

        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        var userManager = _userManagerMock.Object;
        await SeedDatabase(_userManagerMock);
        var repository = new UserRepository(userManager);

        var users = await repository.GetAllAsync();

        Assert.NotNull(users);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        var userManager = _userManagerMock.Object;
        var repository = new UserRepository(userManager);
        var newUser = new User("New User", "newuser@example.com", "Password3!", UserRoles.Admin);

        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.FindByIdAsync(newUser.Id.ToString()))
                        .ReturnsAsync(newUser);

        await repository.AddAsync(newUser, "Password3");

        var user = await userManager.FindByIdAsync(newUser.Id.ToString());
        Assert.NotNull(user);
        Assert.Equal(newUser.UserName, user.UserName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        var userManager = _userManagerMock.Object;
        await SeedDatabase(_userManagerMock);
        var repository = new UserRepository(userManager);
        var user = await userManager.Users.FirstAsync();
        user.Update("Updated User", user.Email, user.Role, "Password3!");

        await repository.UpdateAsync(user);

        var updatedUser = await userManager.FindByIdAsync(user.Id.ToString());
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated User", updatedUser.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkUserAsDeleted()
    {
        var userManager = _userManagerMock.Object;
        await SeedDatabase(_userManagerMock);
        var repository = new UserRepository(userManager);
        var userId = (await userManager.Users.FirstAsync()).Id;

        await repository.DeleteAsync(userId);

        var user = await userManager.FindByIdAsync(userId.ToString());
        Assert.True(user.Deleted);
    }

    [Fact]
    public async Task GetPagedUsersAsync_ShouldReturnPagedUsers()
    {
        var userManager = _userManagerMock.Object;
        await SeedDatabase(_userManagerMock);
        var repository = new UserRepository(userManager);

        var users = await repository.GetPagedUsersAsync(1, 1);

        Assert.NotNull(users);
        Assert.Single(users);
    }
}
