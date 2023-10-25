using System.Security.Claims;
using _9GagClone.Data;
using _9GagClone.Helpers;
using _9GagClone.Services.UserService;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _9GagClone.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;
    private readonly ImageService _imageService;

    public UsersController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment env, ImageService imageService, IUserService userService)
    {
        _userService = userService;
        _context = context;
        _mapper = mapper;
        _env = env;
        _imageService = imageService;
    }

    [Authorize]
    [HttpGet("data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))] 
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<IActionResult> GetUserData()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var user = await _userService.GetUserDataAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<UserDto>(user));
    }
    
    [Authorize]
    [HttpPost("upload-picture")]
    public async Task<IActionResult> UploadPicture(IFormFile file)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest("User ID is invalid.");
        }

        try
        {
            var imageUrl = await _userService.UpdateUserProfilePictureAsync(userId, file);

            if (imageUrl == null)
            {
                return BadRequest("An error occurred while updating profile picture.");
            }

            return Ok(new { ProfilePictureUrl = imageUrl });
        }
        catch (Exception ex)
        {
            // Log the exception (ex)
            return StatusCode(500, "Internal server error occurred.");
        }
    }
}