using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers
{
    public class OrderItemAddRequesttoOrderItemMappingProfile : Profile
    {
        public OrderItemAddRequesttoOrderItemMappingProfile()
        {
            CreateMap<OrderItemAddRequest, OrderItem>()
                .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID));
            CreateMap< OrderItemAddRequest, OrderItem>()
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));
            CreateMap<OrderItemAddRequest, OrderItem>()
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
            CreateMap<OrderItemAddRequest, OrderItem>()
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
            CreateMap<OrderItemAddRequest, OrderItem>()
                .ForMember(dest => dest._id, opt => opt.Ignore());
        }
    }
}
