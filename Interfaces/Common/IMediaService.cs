using Microsoft.AspNetCore.Http;

namespace SkillForge.Interfaces.Common
{
    public interface IMediaService
    {
        string? SaveThumbnail(IFormFile file);
        string HandleVideo(IFormFile? file, string? youtubeUrl, string? videoType);
    }
}
