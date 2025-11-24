namespace courses_buynsell_api.Interfaces;

public interface IImageService
{
    Task<string?> UploadImageAsync(IFormFile file);
    Task DeleteImageAsync(string imageUrl);
}