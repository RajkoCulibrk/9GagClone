namespace _9GagClone.Dtos;

public class CreatePostDto
{
    public string Title { get; set; } 
    public string Content { get; set; } 
    public IFormFile Image { get; set; }
    
}