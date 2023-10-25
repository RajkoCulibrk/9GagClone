using _9GagClone.Data;
using AutoMapper;

namespace _9GagClone.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<User, UserDto>();
        // And other mappings...
    }
}