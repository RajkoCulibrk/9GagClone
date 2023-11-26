using System.Security.Claims;
using _9GagClone.Data;
using _9GagClone.Dtos;
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
    private readonly IMapper _mapper;


    public UsersController(IMapper mapper, IUserService userService)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [Authorize]
    [HttpGet("data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserData()
    {
        var userId = User.GetUserId();

        var user = await _userService.GetUserById(userId);

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

    [Authorize]
    [HttpPost("update-profile")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfileData(UpdateUserDto payload)
    {
        var currentlyLoggedInUserId = User.GetUserId();

        var user = await _userService.GetUserById(currentlyLoggedInUserId);

        await _userService.UpdateProfile(user, payload);

        return Ok(_mapper.Map<UserDto>(user));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FriendDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpPost("unfriend-user/{friendId}")]
    public async Task<IActionResult> AddRemoveFriend(int friendId)
    {
        var userId = User.GetUserId();

        var user = await _userService.UnFriendUser(userId, friendId);


        return Ok(_mapper.Map<FriendDto>(user));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FriendDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpGet("get-friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = User.GetUserId();

        var friends = await _userService.GetFriends(userId);
        var response = _mapper.Map<List<FriendDto>>(friends);

        foreach (var friendship in response)
        {
            friendship.IsFriendsWith = true;
        }


        return Ok(response);
    }
    
    
    [Authorize]
    [HttpPost("make-friend-request/{potentialFriendId}")]
    public async Task<IActionResult> MakeFriendRequest(int potentialFriendId)
    {
        var userId = User.GetUserId();

        var friendRequest = await _userService.CreateFriendRequest(userId,potentialFriendId);


        return Ok(_mapper.Map<GetFriendShipRequestDto>(friendRequest));
    }
    
    [Authorize]
    [HttpPost("accept-friend-request/{requestId}")]
    public async Task<IActionResult> AcceptFriendRequest(int requestId)
    {
        var userId = User.GetUserId();

        var friendRequest = await _userService.AcceptFriendRequest(requestId,userId);

        return Ok(_mapper.Map<GetFriendShipRequestDto>(friendRequest));
    }
    
    [Authorize]
    [HttpPost("decline-friend-request/{requestId}")]
    public async Task<IActionResult> DeclineFriendRequest(int requestId)
    {
        var userId = User.GetUserId();

        var friendRequest = await _userService.DeclineFriendRequest(requestId,userId);


        return Ok(_mapper.Map<GetFriendShipRequestDto>(friendRequest));
    }
    
    [Authorize]
    [HttpDelete("delete-friend-request/{requestId}")]
    public async Task<IActionResult> DeleteFriendRequest(int requestId)
    {
        var userId = User.GetUserId();

        await _userService.DeleteFriendRequest(requestId,userId);
        
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("get-my-friend-requests")]
    public async Task<IActionResult> GetFriendRequestsMadeToMe()
    {
        var userId = User.GetUserId();

        var friendRequests = await _userService.GetAllFriendRequests(userId);
        
        return Ok(_mapper.Map<List<GetFriendShipRequestDto>>(friendRequests));
    }


    [Authorize]
    [HttpGet("get-friend-requests-i-made")]
    public async Task<IActionResult> GetFriendRequestsMadeByMe()
    {
        var userId = User.GetUserId();

        var friendRequests = await _userService.GetAllFriendRequestsIMade(userId);

        return Ok(_mapper.Map<List<GetFriendShipRequestDto>>(friendRequests));
    }
}