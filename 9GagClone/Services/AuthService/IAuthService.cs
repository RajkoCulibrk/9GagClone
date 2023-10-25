using _9GagClone.Dtos;

namespace _9GagClone.Services.AuthService;

public interface IAuthService
{
    Task<bool> RegisterUser(UserRegisterDto userRegisterDto);
    Task<string> LoginUser(UserLoginDto userLoginDto);
}