using Xunit;
using Moq;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Application.UseCases.Orders.Commands.CreateOrder;
using AmxBookstore.Application.DTOs;
using Domain.Entities.Orders;
using Domain.Entities.Users;
using Domain.Entities.Books;
using Domain.Entities.Stocks;
using Microsoft.AspNetCore.Http;
using AmxBookstore.Domain.Entities.Users.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AmxBookstore.Application.Profiles;
using AmxBookstore.Domain.Entities.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IMapper _mapper;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _stockRepositoryMock = new Mock<IStockRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new AutoMapperProfile());
        });

        _mapper = config.CreateMapper();

        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _stockRepositoryMock.Object,
            _bookRepositoryMock.Object,
            _userRepositoryMock.Object,
            _mapper
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenAllConditionsAreMet()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var orderDto = new OrderDTO
        {
            Id = orderId,
            SellerId = userId,
            ClientId = clientId,
            Products = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = bookId, Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand { Order = orderDto, UserId = userId };

        var order = new Order(
            new List<OrderItem> { new OrderItem ( bookId, 1 ) },
            0,
            userId,
            clientId,
            OrderStatus.Created,
            orderId
        );

        var client = new User("Client Name", "client@example.com", "password", UserRoles.Client, clientId);
        var book = new Book("Book Title", "Book Description", 200, "Author Name", 19.99m, bookId);
        var stock = new Stock(bookId, 10);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);
        _stockRepositoryMock.Setup(r => r.GetByProductIdAsync(bookId)).ReturnsAsync(stock);
        _orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(orderId, result);
        _userRepositoryMock.Verify(r => r.GetByIdAsync(clientId), Times.Once);
        _bookRepositoryMock.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _stockRepositoryMock.Verify(r => r.GetByProductIdAsync(bookId), Times.Once);
        _orderRepositoryMock.Verify(r => r.AddAsync(It.Is<Order>(o => o.Id == orderId && o.TotalAmount == book.Price)), Times.Once);
    }


    [Fact]
    public async Task Handle_ShouldThrowException_WhenClientNotFound()
    {
        var orderDto = new OrderDTO
        {
            SellerId = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            Products = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand { Order = orderDto, UserId = Guid.NewGuid() };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User)null);

        await Assert.ThrowsAsync<BadHttpRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenBookNotFound()
    {
        var clientId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var orderDto = new OrderDTO
        {
            SellerId = Guid.NewGuid(),
            ClientId = clientId,
            Products = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = bookId, Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand { Order = orderDto, UserId = Guid.NewGuid() };

        var client = new User("Client Name", "client@example.com", "password", UserRoles.Client, clientId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync((Book)null);

        await Assert.ThrowsAsync<BadHttpRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStockNotFound()
    {
        var clientId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var orderDto = new OrderDTO
        {
            SellerId = Guid.NewGuid(),
            ClientId = clientId,
            Products = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = bookId, Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand { Order = orderDto, UserId = Guid.NewGuid() };

        var client = new User("Client Name", "client@example.com", "password", UserRoles.Client, clientId);
        var book = new Book("Book Title", "Book Description", 200, "Author Name", 19.99m, bookId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);
        _stockRepositoryMock.Setup(r => r.GetByProductIdAsync(bookId)).ReturnsAsync((Stock)null);

        await Assert.ThrowsAsync<BadHttpRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStockQuantityIsInsufficient()
    {
        var clientId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var orderDto = new OrderDTO
        {
            SellerId = Guid.NewGuid(),
            ClientId = clientId,
            Products = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = bookId, Quantity = 10 }
            }
        };

        var command = new CreateOrderCommand { Order = orderDto, UserId = Guid.NewGuid() };

        var client = new User("Client Name", "client@example.com", "password", UserRoles.Client, clientId);
        var book = new Book("Book Title", "Book Description", 200, "Author Name", 19.99m, bookId);
        var stock = new Stock(bookId, 5);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);
        _stockRepositoryMock.Setup(r => r.GetByProductIdAsync(bookId)).ReturnsAsync(stock);

        await Assert.ThrowsAsync<BadHttpRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }


}
