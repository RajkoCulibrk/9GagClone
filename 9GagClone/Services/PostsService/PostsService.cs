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
            .Include(p=>p.Reactions)
            .FirstOrDefaultAsync(p => p.Id == createdPost.Entity.Id);

        return refetchedPost;
    }

    public async Task<Post> GetPost(int postId)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p=>p.Reactions)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null) throw new Exception($"Post with id {postId} was not found");

        return post;
    }

    public async Task<List<Post>> GetMyPosts(int userId)
    {
        var myPosts = await _context.Posts
            .Include(p => p.User)
            .Include(p=>p.Reactions)
            .Where(p => p.UserId == userId)
            .ToListAsync();
        return myPosts;
    }

    public async Task<List<Post>> GetAllPosts()
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p=>p.Reactions)
            .ToListAsync();
    }

    public async Task<List<Post>> GetPostsMyFriendLikes(int userId, int friendId)
    {
        var user = await _context.Users.Include(u => u.Friends).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception($"User with id: {userId} was not found");

        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);
        if (friend == null) throw new Exception($"User with id: {friendId} was not found");

        this.CheckFriendshipRaiseError(user, friend);

        // Fetch posts that the friend has liked.
        var postsFriendLikes = await _context.UserReactions
            .Include(r => r.Post)   // Include the related post
            .ThenInclude(p => p.User) // Include the user of the post (if necessary)
            .Where(r => r.UserId == friend.Id && r.Reaction == ReactionType.Like)  // Assuming ReactionType.Like represents a like
            .Select(r => r.Post)  // Select the post associated with the reaction
            .ToListAsync();

        return postsFriendLikes;
    }

    public async Task<List<Post>> GetPostsOfAUser(int userId)
    {
        return await _context.Posts
            .Include(p=>p.User)
            .Include(p => p.Reactions)
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    private void CheckFriendshipRaiseError(User user, User friend)
    {
        var foundFriend = user.Friends.FirstOrDefault(f => f.Friend.Id == friend.Id);

        if (foundFriend == null)
        {
            throw new Exception($"You are not friends with user whose id is{friend.Id}");
        }
    }
    
    public async Task<UserPostReaction> GetUserReactionForPost(int userId, int postId)
    {
        return await _context.UserReactions.FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);
    }

    public async Task<Post> LikeOrDislikeAPost(int userId, int postId, ReactionType newReaction)
    {
        // Check if a reaction already exists for the user on this post
        var existingReaction = await _context.UserReactions
            .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);

        // No previous reaction found, so add the new reaction
        if (existingReaction == null)
        {
            var reactionToAdd = new UserPostReaction
            {
                UserId = userId,
                PostId = postId,
                Reaction = newReaction,
                ReactedAt = DateTime.UtcNow
            };
            _context.UserReactions.Add(reactionToAdd);
        }
        else
        {
            // If the new reaction matches the existing one, remove the existing reaction
            if (existingReaction.Reaction == newReaction)
            {
                _context.UserReactions.Remove(existingReaction);
            }
            // Else, update the reaction
            else
            {
                existingReaction.Reaction = newReaction;
                existingReaction.ReactedAt = DateTime.UtcNow; // Update reaction time
            }
        }

        await _context.SaveChangesAsync();
        
        var refetched = await _context.Posts
            .Include(p=>p.User)
            .Include(p => p.Reactions)
            .FirstOrDefaultAsync(p => p.Id == postId);

        // Optionally, return the post. You might want to include related data or just return the updated post details.
        return refetched;
    }


}