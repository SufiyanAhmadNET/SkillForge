using Microsoft.AspNetCore.Http;

namespace SkillForge.Interfaces
{
    public interface IMediaService
    {
        string? SaveThumbnail(IFormFile file);
        string HandleVideo(IFormFile? file, string? youtubeUrl, string? videoType);
        string? UploadResume(IFormFile file);
        string? SaveProfilePhoto(IFormFile file);
    }
}
