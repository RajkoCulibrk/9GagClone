using _9GagClone.Data;
using _9GagClone.Dtos;
using AutoMapper;

namespace _9GagClone.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<User, UserDto>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<User, FriendDto>();
        CreateMap<Post, GetPostDto>()
            .ForMember(
                dest => dest.User,
                opt => opt.MapFrom(src => src.User
                ));
        CreateMap<FriendRequest, GetFriendShipRequestDto>()
            .ForMember(dest => dest.Requester, opt => opt.MapFrom(src => src.Requester))
            .ForMember(dest => dest.Requested, opt => opt.MapFrom(src => src.Requested))
            .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src => src.RequestDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        // And other mappings...
    }
}