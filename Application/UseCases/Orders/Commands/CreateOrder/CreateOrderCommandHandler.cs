using MediatR;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Domain.Entities.Orders;
using Microsoft.AspNetCore.Http;
using AmxBookstore.Domain.Entities.Users.Enum;

namespace AmxBookstore.Application.UseCases.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CreateOrderCommandHandler(IOrderRepository orderRepository,
                                         IStockRepository stockRepository,
                                         IBookRepository bookRepository,
                                         IUserRepository userRepository,
                                         IMapper mapper)
        {
            _orderRepository = orderRepository;
            _stockRepository = stockRepository;
            _userRepository = userRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            request.Order.Status = "Created";
            request.Order.SellerId = request.UserId;

            var order = _mapper.Map<Order>(request.Order);

            var client = await _userRepository.GetByIdAsync(order.ClientId);

            if (client == null)
            {
                throw new BadHttpRequestException("Client not founded");
            }

            if (client.Role != UserRoles.Client)
            {
                throw new BadHttpRequestException("Orders can only be created to clients.");
            }


            decimal totalAmount = 0;

            foreach (var product in order.Products)
            {
                var book = await _bookRepository.GetByIdAsync(product.ProductId);

                if (book == null)
                {
                    throw new BadHttpRequestException("Book not founded");
                }

                var stock = await _stockRepository.GetByProductIdAsync(product.ProductId);

                if (stock == null)
                {
                    throw new BadHttpRequestException("Stock not founded");
                }

                if (stock.Quantity < product.Quantity)
                {
                    throw new BadHttpRequestException("Requested quantity is bigger than the stock");
                }

                if (product.Quantity <= 0)
                {
                    throw new BadHttpRequestException("Requested quantity should be bigger than 0");
                }

                totalAmount += book.Price * product.Quantity;

                stock.UpdateQuantity(stock.Quantity - product.Quantity);

                await _stockRepository.UpdateAsync(stock);
            }

            order.InsertTotalAmount(totalAmount);

            await _orderRepository.AddAsync(order);
            return order.Id;
        }
    }
}
