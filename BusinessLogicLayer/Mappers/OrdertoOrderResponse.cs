using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers
{
    public class OrdertoOrderResponseMappingProfile : Profile
    {
        public OrdertoOrderResponseMappingProfile()
        {
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID));
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate));
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => src.OrderID));
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.TotalBill, opt => opt.MapFrom(src => src.TotalBill));
            //CreateMap<Order, OrderResponse>()
            //    .ForMember(dest => dest.Ignore(), opt => opt.MapFrom(src => src._id));
        }
    }
}
