namespace _9GagClone.Data;

public class Post
{
    public int Id { get; set; } // Unique identifier for the post

    public int UserId { get; set; } 
    public User User { get; set; }

    public string Title { get; set; } 
    public string Content { get; set; } 

    public DateTime CreatedAt { get; set; } 
    public DateTime? UpdatedAt { get; set; } 

    public string ImageUrl { get; set; } 
    
    public ICollection<UserPostReaction> Reactions { get; set; }
}