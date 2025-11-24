using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using courses_buynsell_api.Interfaces;

namespace courses_buynsell_api.Services;

public class ImageService(Cloudinary cloudinary) : IImageService
{
    public async Task<string?> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            Folder = "courses"
        };
        
        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        return uploadResult?.SecureUrl?.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;
        
        var publicId = GetPublicIdFromUrl(imageUrl);
        if (publicId == null)
            return;
        await cloudinary.DestroyAsync(new DeletionParams(publicId));

    }
    
    private string? GetPublicIdFromUrl(string url)
    {
        // Example: https://res.cloudinary.com/.../courses/abc123.jpg
        var parts = url.Split('/');
        var fileName = parts.Last();

        if (fileName.Contains('.'))
            fileName = fileName[..fileName.LastIndexOf('.')];

        return "courses/" + fileName;
    }
}