using _9GagClone.Data;

namespace _9GagClone.Dtos;

public class GetPostDto
{
    public int Id { get; set; }
    public FriendDto User { get; set; }

    public string Title { get; set; } 
    public string Content { get; set; } 

    public DateTime CreatedAt { get; set; } 
    public DateTime? UpdatedAt { get; set; } 

    public string ImageUrl { get; set; } 
}