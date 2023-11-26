using _9GagClone.Data;
using _9GagClone.Dtos;
using _9GagClone.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace _9GagClone.Services.UserService;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ImageService _imageService;

    public UserService(ApplicationDbContext context, IMapper mapper, ImageService imageService)
    {
        _context = context;
        _mapper = mapper;
        _imageService = imageService;
    }

    public async Task<User?> GetUserById(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user;
    }

    public async Task<string> UpdateUserProfilePictureAsync(int userId, IFormFile file)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            // Handle user not found scenario (you could throw an exception or handle it accordingly)
            return null;
        }

        if (file == null || file.Length == 0)
        {
            // Handle the bad file scenario (you could throw an exception or handle it accordingly)
            return null;
        }

        if (!file.ContentType.Contains("image"))
        {
            // Handle the non-image file scenario (you could throw an exception or handle it accordingly)
            return null;
        }

        // Delete old image if it exists.
        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            _imageService.DeleteImage(user.ProfilePictureUrl);
        }

        // Save the new image and get the path.
        string imageUrl = await _imageService.SaveImageAsync(file);

        user.ProfilePictureUrl = imageUrl;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return user.ProfilePictureUrl;
    }

    public async Task<User> UpdateProfile(User user, UpdateUserDto payload)
    {
        var updated = _mapper.Map(payload, user);
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return updated;
    }

    public async Task<User> UnFriendUser(int userId, int friendId)
    {

        var user = await _context.Users.Include(u => u.Friends).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception($"User with id: {userId} was not found");
        }

        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);
        if (friend == null)
        {
            throw new Exception($"User with id: {friendId} was not found");
        }

        var existingFriendship = await _context.Friendships
            .Include(f=>f.Friend)
            .Include(f=>f.User)
            .FirstOrDefaultAsync(f=>f.User == user && f.Friend == friend);
        
        var existingFriendship2 = await _context.Friendships
            .Include(f=>f.Friend)
            .Include(f=>f.User)
            .FirstOrDefaultAsync(f=>f.User == friend && f.Friend == user);

        if (existingFriendship == null || existingFriendship2 == null)
        {
            throw new Exception($"You are not friends with user with id: {friendId}");
        }
        else
        {
            _context.Friendships.Remove(existingFriendship);
            _context.Friendships.Remove(existingFriendship2);
        }

        await _context.SaveChangesAsync();
        return friend;
    }

    public async Task<List<User>> GetFriends(int userId)
    {
        var user = await this.GetUserById(userId);

        if (user == null)
        {
            throw new Exception($"User with id not found ${userId}");
        }

        var friends = await _context.Friendships
            .Where(f => f.UserId == user.Id).Select(f => f.Friend)
            .ToListAsync();

        return friends;
    }

    public async Task<FriendRequest> CreateFriendRequest(int userId, int potentialFriendId)
    {
        var existing = await _context.FriendshipRequests
            .FirstOrDefaultAsync(fr =>
                fr.RequestedId == potentialFriendId && fr.RequesterId == userId &&
                fr.Status == FriendShipRequestsStatus.Pending);

        if (existing != null)
        {
            throw new Exception(
                "You have already sent a friend request to this user which the user has not yet accepted or declined");
        }

        var existsingFriendShip = await _context.Friendships
            .Include(f => f.Friend)
            .FirstOrDefaultAsync(f=>f.Friend.Id==potentialFriendId && f.UserId == userId);

        if (existsingFriendShip != null)
        {
            throw new Exception(
                "You are already friends");
        }

        var newRequest = new FriendRequest()
        {
            RequestedId = potentialFriendId,
            RequesterId = userId,
            Status = FriendShipRequestsStatus.Pending,
            RequestDate = DateTime.UtcNow
        };

        var saved = await _context.FriendshipRequests.AddAsync(newRequest);
        await _context.SaveChangesAsync();

        var refetched = await _context.FriendshipRequests
            .Include(fr => fr.Requested)
            .Include(fr => fr.Requester)
            .FirstOrDefaultAsync(fr => fr.Id == saved.Entity.Id);

        return refetched;
    }

    public async Task<FriendRequest> AcceptFriendRequest(int requestId, int userId)
    {
        var user = await this.GetUserById(userId);

        if (user == null) throw new Exception($"User with id: {userId} was not found");

        var friendRequest =
            await _context.FriendshipRequests.Include(fr=>fr.Requester).FirstOrDefaultAsync(fr => fr.Id == requestId && fr.RequestedId == userId);

        if (friendRequest == null) throw new Exception($"Request with id: {requestId} was not found");

        friendRequest.Status = FriendShipRequestsStatus.Accepted;

        var newFriendship1 = new Friendship()
        {
            Friend = friendRequest.Requester,
            User = user,
        };
        
        var newFriendship2 = new Friendship()
        {
            Friend = user,
            User = friendRequest.Requester ,
        };

        await _context.Friendships.AddAsync(newFriendship1);
        await _context.Friendships.AddAsync(newFriendship2);

        await _context.SaveChangesAsync();
        

        return friendRequest;
    }

    public async Task<FriendRequest> DeclineFriendRequest(int requestId, int userId)
    {
        var request = await
            _context.FriendshipRequests
                .Include(fr=>fr.Requested)
                .Include(fr=>fr.Requester)
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.RequestedId == userId && fr.Status == FriendShipRequestsStatus.Pending);

        if (request == null) throw new Exception($"Request was not found");
        if (request.Status == FriendShipRequestsStatus.Accepted || request.Status == FriendShipRequestsStatus.Declined)
        {
            throw new Exception($"Request must be in pending state");
        }

        request.Status = FriendShipRequestsStatus.Declined;

        await _context.SaveChangesAsync();

        return request;
    }


    public async Task<List<FriendRequest>> GetAllFriendRequests(int userId,
        FriendShipRequestsStatus? status = FriendShipRequestsStatus.Pending)
    {
        var user = await this.GetUserById(userId);

        if (user == null) throw new Exception($"User with id: {userId} was not found");
        var friendRequests = await _context.FriendshipRequests
            .Include(fr=>fr.Requested)
            .Include(fr=>fr.Requester)
            .Where(fr => fr.RequestedId == user.Id && fr.Status == status).ToListAsync();

        return friendRequests;
    }

    public async Task<List<FriendRequest>> GetAllFriendRequestsIMade(int userId,
    FriendShipRequestsStatus? status = FriendShipRequestsStatus.Pending)
    {
        var user = await this.GetUserById(userId);

        if (user == null) throw new Exception($"User with id: {userId} was not found");
        var friendRequests = await _context.FriendshipRequests
            .Include(fr => fr.Requested)
            .Include(fr => fr.Requester)
            .Where(fr => fr.RequesterId == user.Id && fr.Status == status).ToListAsync();

        return friendRequests;
    }

    public async Task DeleteFriendRequest(int requestId, int userId)
    {
        var user = await this.GetUserById(userId);

        if (user == null) throw new Exception($"User with id: {userId} was not found");

        var friendRequest =
            await _context.FriendshipRequests.FirstOrDefaultAsync(fr => fr.Id == requestId && fr.RequesterId == userId && fr.Status == FriendShipRequestsStatus.Pending);

        if (friendRequest == null) throw new Exception($"Request was not found");

        _context.FriendshipRequests.Remove(friendRequest);
        await _context.SaveChangesAsync();
    }
}