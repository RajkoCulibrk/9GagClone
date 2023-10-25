using _9GagClone.Data;

namespace _9GagClone.Services.UserService;

public interface IUserService
{
    Task<User> GetUserDataAsync(string userId);
    Task<string>  UpdateUserProfilePictureAsync(int userId, IFormFile file);
}