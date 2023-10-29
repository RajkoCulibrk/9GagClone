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

        return Ok(_mapper.Map<GetPostDto>(post));
    }
    
    [HttpGet("{postId}")]
    public async Task<IActionResult> GetPost(int postId)
    {
        var post = _postsService.GetPost(postId);

        return Ok(_mapper.Map<GetPostDto>(post));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        var posts = await _postsService.GetAllPosts();

        return Ok(_mapper.Map<List<GetPostDto>>(posts));
    }
    
    [HttpGet("belonging-to/{userId}")]
    public async Task<IActionResult> GetPostsOfAUser(int userId)
    {
        var usersPosts = await _postsService.GetPostsOfAUser(userId);

        return Ok(usersPosts);
    }
    
    [Authorize]
    [HttpGet("liked-by-friend/{friendId}")]
    public async Task<IActionResult> GetPostsMyFriendLikes(int friendId)
    {
        var userId = User.GetUserId();
        var usersPosts = await _postsService.GetPostsMyFriendLikes(userId,friendId);

        return Ok(usersPosts);
    }
    
    
}