using System.Text;
using _9GagClone.Data;
using _9GagClone.Helpers;
using _9GagClone.Services.AuthService;
using _9GagClone.Services.PostsService;
using _9GagClone.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();

builder.Services.AddSingleton(jwtConfig);
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddScoped<ImageService>(provider => 
    new ImageService(builder.Environment.ContentRootPath));
builder.Services.AddScoped<IUserService, UserService>(); 
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<IPostsService, PostsService>(); 

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),

            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string staticFilesDirectory = Path.Combine(app.Environment.ContentRootPath, "MyStaticFiles");
if (!Directory.Exists(staticFilesDirectory))
{
    // If the directory doesn't exist, create it.
    Directory.CreateDirectory(staticFilesDirectory);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staticFilesDirectory),
    RequestPath = "/StaticFiles"
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();