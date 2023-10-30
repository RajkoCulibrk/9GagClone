using _9GagClone.Data;
using _9GagClone.Dtos;
using _9GagClone.Helpers;
using _9GagClone.Services.PostsService;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _9GagClone.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostsService _postsService;
    private readonly IMapper _mapper;

    public PostsController(IPostsService postsService, IMapper mapper)
    {
        _postsService = postsService;
        _mapper = mapper;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostDto createPostDto)
    {
        var userId = User.GetUserId();
        var post = await _postsService.CreatePost(createPostDto, userId);
        var postDto = _mapper.Map<GetPostDto>(post);

        if (User.Identity.IsAuthenticated)
        {
            await EnrichUserReaction(postDto, userId);
        }

        return Ok(_mapper.Map<GetPostDto>(post));
    }

    [HttpGet("{postId}")]
    public async Task<IActionResult> GetPost(int postId)
    {
        var post =  await _postsService.GetPost(postId);
        var postDto = _mapper.Map<GetPostDto>(post);

        if (User.Identity.IsAuthenticated)
        {
            await EnrichUserReaction(postDto, User.GetUserId());
        }


        return Ok(postDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        var posts = await _postsService.GetAllPosts();
        var postDtos = _mapper.Map<List<GetPostDto>>(posts);

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.GetUserId();
            await EnrichUserReaction(postDtos, userId);
        }

        return Ok(postDtos);
    }

    [HttpGet("belonging-to/{userId}")]
    public async Task<IActionResult> GetPostsOfAUser(int userId)
    {
        var usersPosts = await _postsService.GetPostsOfAUser(userId);
        var postDtos = _mapper.Map<List<GetPostDto>>(usersPosts);

        if (User.Identity.IsAuthenticated)
        {
            var loggedInUserId = User.GetUserId();
            await EnrichUserReaction(postDtos, loggedInUserId);
        }

        return Ok(postDtos);
    }

    [Authorize]
    [HttpGet("liked-by-friend/{friendId}")]
    public async Task<IActionResult> GetPostsMyFriendLikes(int friendId)
    {
        var userId = User.GetUserId();
        var usersPosts = await _postsService.GetPostsMyFriendLikes(userId, friendId);
        var postDtos = _mapper.Map<List<GetPostDto>>(usersPosts);
        if (User.Identity.IsAuthenticated)
        {
            await EnrichUserReaction(postDtos, userId);
        }

        return Ok(postDtos);
    }

    [Authorize]
    [HttpPost("like-dislike/{postId}")]
    public async Task<IActionResult> LikeDislikePost(int postId, LikeDislikePostDto reactionDto)
    {
        var userId = User.GetUserId();
        var post = await _postsService.LikeOrDislikeAPost(userId, postId, reactionDto.Reaction);
        var postDto = _mapper.Map<GetPostDto>(post);

        if (User.Identity.IsAuthenticated)
        {
            await EnrichUserReaction(postDto, userId);
        }

        return Ok(postDto);
    }

    private async Task EnrichUserReaction(GetPostDto post, int userId)
    {
        var reaction = await _postsService.GetUserReactionForPost(userId, post.Id);
        if (reaction != null)
        {
            post.UserReaction = reaction.Reaction;
        }
    }

    private async Task EnrichUserReaction(IEnumerable<GetPostDto> posts, int userId)
    {
        foreach (var post in posts)
        {
            var reaction = await _postsService.GetUserReactionForPost(userId, post.Id);
            if (reaction != null)
            {
                post.UserReaction = reaction.Reaction;
            }
        }
    }
}