using _9GagClone.Data;
using _9GagClone.Helpers;
using AutoMapper;

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

    public async Task<User> GetUserDataAsync(string userId)
    {
        var user = await _context.Users.FindAsync(Convert.ToInt32(userId));
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
}