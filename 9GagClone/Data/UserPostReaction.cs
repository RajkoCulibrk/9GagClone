namespace _9GagClone.Data;

public class UserPostReaction
{
    public int Id { get; set; } 

    public int UserId { get; set; } 
    public User User { get; set; } 

    public int PostId { get; set; }
    public Post Post { get; set; } 

    public ReactionType Reaction { get; set; }

    public DateTime ReactedAt { get; set; } 
}