namespace _9GagClone.Data;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] PasswordHash { get; set; }
    public string? ProfilePictureUrl { get; set; }
}