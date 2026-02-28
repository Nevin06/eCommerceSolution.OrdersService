using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers;

public class ProductDTOtoOrderItemResponseMappingProfile : Profile
{
    public ProductDTOtoOrderItemResponseMappingProfile()
    {
        //CreateMap<ProductDTO, OrderItemResponse>()
        //    .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID));
        //CreateMap<ProductDTO, OrderItemResponse>()
        //    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
        //CreateMap<ProductDTO, OrderItemResponse>()
        //    .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));
        //CreateMap<ProductDTO, OrderItemResponse>()
        //    .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
        CreateMap<ProductDTO, OrderItemResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName));
        CreateMap<ProductDTO, OrderItemResponse>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        // remaining properties will be untouched, they contain the same values
    }
}
