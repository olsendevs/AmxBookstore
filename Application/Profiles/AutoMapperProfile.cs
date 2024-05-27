using AmxBookstore.Application.DTOs;
using AmxBookstore.Domain.Entities.Orders;
using AutoMapper;
using Domain.Entities.Books;
using Domain.Entities.Orders;
using Domain.Entities.Stocks;
using Domain.Entities.Users;

namespace AmxBookstore.Application.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<Book, BookDTO>().ReverseMap();
            CreateMap<Stock, StockDTO>().ReverseMap();
            CreateMap<OrderItem, OrderItemDTO>().ReverseMap();
            CreateMap<Order, OrderDTO>().ReverseMap();
 
        }
    }
}
