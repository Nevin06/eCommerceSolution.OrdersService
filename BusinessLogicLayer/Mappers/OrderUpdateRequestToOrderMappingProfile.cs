using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.DataAccessLayer.Entities;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers
{
    public class OrderUpdateRequesttoOrderMappingProfile : Profile
    {
        public OrderUpdateRequesttoOrderMappingProfile()
        {
            CreateMap<OrderUpdateRequest, Order>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID));
            CreateMap<OrderUpdateRequest, Order>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate));
            CreateMap<OrderUpdateRequest, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<OrderUpdateRequest, Order>()
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => src.OrderID));
            CreateMap<OrderUpdateRequest, Order>()
                .ForMember(dest => dest.TotalBill, opt => opt.Ignore());
            CreateMap<OrderUpdateRequest, Order>()
                .ForMember(dest => dest._id, opt => opt.Ignore());
        }
    }
}
