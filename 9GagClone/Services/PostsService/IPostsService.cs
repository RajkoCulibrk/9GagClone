using _9GagClone.Data;
using _9GagClone.Dtos;

namespace _9GagClone.Services.PostsService;

public interface IPostsService
{
    Task<Post> CreatePost(CreatePostDto createPostDto, int userId);
    Task<Post> GetPost( int postId);
    
    Task<List<Post>> GetMyPosts(int userId);
    Task<List<Post>> GetAllPosts();
    Task<List<Post>> GetPostsMyFriendLikes(int userId, int friendId);
    Task<List<Post>> GetPostsOfAUser(int userId);
    Task<UserPostReaction> GetUserReactionForPost(int userId, int postId);
    Task<Post> LikeOrDislikeAPost(int userId, int postId, ReactionType reaction);
    Task<bool> CheckIsFriendsWith(int userId, int postId);

}