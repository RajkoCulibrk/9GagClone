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
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
        // And other mappings...
    }
}