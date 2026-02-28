using AutoMapper;
using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers;

public class UserDTOtoOrderResponseMappingProfile : Profile
{
    public UserDTOtoOrderResponseMappingProfile()
    {
        CreateMap<UserDTO, OrderResponse>().ForMember(dest => dest.UserPersonName, opt => opt.MapFrom(src => src.PersonName));
        CreateMap<UserDTO, OrderResponse>().ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
    }
}
