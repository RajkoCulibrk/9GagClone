using _9GagClone.Data;
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

    public async Task<User> AddRemoveFriend(int userId, int friendId)
    {
        if (userId == friendId)
        {
            throw new Exception("You cannot befriend yourself");
        }
        
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

        var existingFriendship = user.Friends.FirstOrDefault(f => f.Friend == friend);

        if (existingFriendship == null)
        {
            var newFriendship = new Friendship()
            {
                Friend = friend,
                User = user
            };

            user.Friends.Add(newFriendship);
        }
        else
        {
            user.Friends.Remove(existingFriendship);
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
}