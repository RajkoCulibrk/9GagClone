namespace _9GagClone.Helpers;

public class ImageService
{
    private readonly string _imageFolderPath;

    public ImageService(string contentRootPath)
    {
        _imageFolderPath = Path.Combine(contentRootPath, "MyStaticFiles");
        if (!Directory.Exists(_imageFolderPath))
        {
            Directory.CreateDirectory(_imageFolderPath);
        }
    }

    public async Task<string> SaveImageAsync(IFormFile file)
    {
        string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(_imageFolderPath, newFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/StaticFiles/{newFileName}";
    }

    public void DeleteImage(string imageUrl)
    {
        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(_imageFolderPath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}