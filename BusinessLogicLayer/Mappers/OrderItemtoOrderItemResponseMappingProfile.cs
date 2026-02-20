using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers
{
    public class OrderItemtoOrderItemResponseMappingProfile : Profile
    {
        public OrderItemtoOrderItemResponseMappingProfile()
        {
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID));
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));
            //CreateMap<OrderItem, OrderItemResponse>()
            //    .ForMember(dest => dest._id, opt => opt.MapFrom(src => src._id));
        }
    }
}
