using _9GagClone.Data;

namespace _9GagClone.Services.UserService;

public interface IUserService
{
    Task<User> GetUserById(int userId);
    Task<string>  UpdateUserProfilePictureAsync(int userId, IFormFile file);
    Task<User> UpdateProfile (User user, UpdateUserDto payload);
    Task<User> AddRemoveFriend (int userId, int friendId);
    Task<List<User>> GetFriends (int userId);
}