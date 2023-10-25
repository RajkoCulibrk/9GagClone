using _9GagClone.Data;
using _9GagClone.Dtos;
using _9GagClone.Helpers;
using Microsoft.EntityFrameworkCore;

namespace _9GagClone.Services.AuthService;

public class AuthService:IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly JwtHelper _jwtHelper;

    public AuthService(ApplicationDbContext context, IConfiguration configuration,JwtHelper jwtHelper)
    {
        _context = context;
        _configuration = configuration;
        _jwtHelper = jwtHelper;
    }

    public async Task<bool> RegisterUser(UserRegisterDto userRegisterDto)
    {
        if (await UserExists(userRegisterDto.Email))
        {
            // User already exists. Return false or handle as appropriate (e.g., throw an exception).
            return false;
        }

        var user = new User()
        {
            FirstName = userRegisterDto.FirstName,
            LastName = userRegisterDto.LastName,
            Email = userRegisterDto.Email,
            CreatedAt = DateTime.UtcNow, // Consider using DateTime.Now if appropriate for your situation.
            UpdatedAt = DateTime.UtcNow
        };

        var pepper = _configuration.GetValue<string>("Pepper");
        user.PasswordHash = AuthHelper.CreatePasswordHash(userRegisterDto.Password, pepper);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return true; // Registration successful.
    }

    private async Task<bool> UserExists(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }
    
    public async Task<string> LoginUser(UserLoginDto userLoginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == userLoginDto.Email);

        if (user == null)
        {
            // Instead of returning an ActionResult, you throw an exception or return a specific type/result indicating failure.
            throw new UnauthorizedAccessException("Invalid email");
        }

        var pepper = _configuration["Pepper"]; 

        if (!AuthHelper.VerifyPasswordHash(userLoginDto.Password, user.PasswordHash, pepper))
        {
            throw new UnauthorizedAccessException("Invalid password");
        }

        var token = _jwtHelper.GenerateToken(user);

        return token;
    }
}