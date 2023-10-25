using _9GagClone.Data;
using _9GagClone.Dtos;
using _9GagClone.Helpers;
using _9GagClone.Services.AuthService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _9GagClone.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly IConfiguration _configuration;
    private readonly IAuthService _authService;

    public AuthController(ApplicationDbContext context, JwtHelper jwtHelper, IConfiguration configuration,IAuthService authService)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _configuration = configuration;
        _authService = authService;
    }
    
    [HttpPost("Register")]
    public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
    {
        try
        {
            var result = await _authService.RegisterUser(userRegisterDto);

            if (!result)
            {
                return BadRequest("Email is already in use");
            }

            return Ok(); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error occurred.");
        }
    }
    
    private async Task<bool> UserExists(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }
    
    [HttpPost("Login")]
    public async Task<IActionResult> Login(UserLoginDto userLoginDto)
    {
        try
        {
            var token = await _authService.LoginUser(userLoginDto);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(e.Message); 
        }
    }
    
    
}