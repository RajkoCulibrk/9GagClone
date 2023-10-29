using _9GagClone.Data;
using _9GagClone.Dtos;
using _9GagClone.Helpers;
using Microsoft.EntityFrameworkCore;

namespace _9GagClone.Services.PostsService;

public class PostsService : IPostsService
{
    private readonly ApplicationDbContext _context;
    private readonly ImageService _imageService;

    public PostsService(ApplicationDbContext context, ImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<Post> CreatePost(CreatePostDto createPostDto, int userId)
    {
        var newPost = new Post()
        {
            UserId = userId,
            Title = createPostDto.Title,
            Content = createPostDto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var imageUrl = await _imageService.SaveImageAsync(createPostDto.Image);
        newPost.ImageUrl = imageUrl;

        var createdPost = await _context.Posts.AddAsync(newPost);
        
        await _context.SaveChangesAsync();

        var refetchedPost = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == createdPost.Entity.Id);

        return refetchedPost;
    }

    public async Task<Post> GetPost(int postId)
    {
        var post = await _context.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null) throw new Exception($"Post with id {postId} was not found");

        return post;
    }

    public async Task<List<Post>> GetMyPosts(int userId)
    {
        var myPosts = await _context.Posts.Include(p => p.User).Where(p => p.UserId == userId).ToListAsync();
        return myPosts;
    }

    public async Task<List<Post>> GetAllPosts()
    {
        return await _context.Posts.Include(p => p.User).ToListAsync();
    }

    public async Task<List<Post>> GetPostsMyFriendLikes(int userId, int friendId)
    {
        var user = await _context.Users.Include(u => u.Friends).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception($"User with id: ${userId} was not found");

        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);
        if (friend == null) throw new Exception($"User with id: ${userId} was not found");

        this.CheckFriendshipRaiseError(user, friend);

        var friendsPosts = await _context.Posts.Where(p => p.UserId == friend.Id).ToListAsync();

        return friendsPosts;
    }

    public async Task<List<Post>> GetPostsOfAUser(int userId)
    {
        return await _context.Posts.Where(p => p.UserId == userId).ToListAsync();
    }

    private void CheckFriendshipRaiseError(User user, User friend)
    {
        var foundFriend = user.Friends.FirstOrDefault(f => f.Friend.Id == friend.Id);

        if (foundFriend == null)
        {
            throw new Exception($"You are not friends with user whose id is{friend.Id}");
        }
    }
}