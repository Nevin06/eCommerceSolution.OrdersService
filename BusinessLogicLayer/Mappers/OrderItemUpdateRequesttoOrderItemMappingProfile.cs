using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers
{
    public class OrderItemUpdateRequesttoOrderItemMappingProfile : Profile
    {
        public OrderItemUpdateRequesttoOrderItemMappingProfile()
        {
            CreateMap<OrderItemUpdateRequest, OrderItem>()
                .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID));
            CreateMap<OrderItemUpdateRequest, OrderItem>()
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));
            CreateMap<OrderItemUpdateRequest, OrderItem>()
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
            CreateMap<OrderItemUpdateRequest, OrderItem>()
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
            CreateMap<OrderItemUpdateRequest, OrderItem>()
                .ForMember(dest => dest._id, opt => opt.Ignore());
        }
    }
}
