using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers
{
    public class OrderAddRequesttoOrderMappingProfile : Profile
    {
        public OrderAddRequesttoOrderMappingProfile() 
        { 
            CreateMap<OrderAddRequest, Order>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID));
            CreateMap<OrderAddRequest, Order>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate));
            CreateMap<OrderAddRequest, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<OrderAddRequest, Order>()
                .ForMember(dest => dest.OrderID, opt => opt.Ignore());
            CreateMap<OrderAddRequest, Order>()
                .ForMember(dest => dest.TotalBill, opt => opt.Ignore());
            CreateMap<OrderAddRequest, Order>()
                .ForMember(dest => dest._id, opt => opt.Ignore());
        }
    }
}
