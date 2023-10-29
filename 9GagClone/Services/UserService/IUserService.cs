using _9GagClone.Data;
using _9GagClone.Dtos;

namespace _9GagClone.Services.UserService;

public interface IUserService
{
    Task<User> GetUserById(int userId);
    Task<string> UpdateUserProfilePictureAsync(int userId, IFormFile file);
    Task<User> UpdateProfile(User user, UpdateUserDto payload);
    Task<User> UnFriendUser(int userId, int friendId);
    Task<List<User>> GetFriends(int userId);
    Task<FriendRequest> CreateFriendRequest(int userId, int potentialFriendId);
    Task<FriendRequest> AcceptFriendRequest(int requestId, int userId);
    Task<FriendRequest> DeclineFriendRequest(int requestId, int userId);
    Task<List<FriendRequest>> GetAllFriendRequests(int userId, FriendShipRequestsStatus? status = FriendShipRequestsStatus.Pending);
    Task DeleteFriendRequest(int requestId, int userId);
}