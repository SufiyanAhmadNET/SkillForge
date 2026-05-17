using SkillForge.Interfaces;

namespace SkillForge.Services.Common
{
    public class MediaService : IMediaService
    {
        private readonly IWebHostEnvironment _env;
        public MediaService(IWebHostEnvironment env)
        {
            _env = env;
        }
        // SaveThumbnail                       
        public string? SaveThumbnail(IFormFile file)
        {
            // image limits
            long maxSize = 2 * 1024 * 1024; // 2MB
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            // if file exists
            if (file == null || file.Length == 0)
            {
                return null; // No file uploaded
            }
            //  file size
            if (file.Length > maxSize)
                throw new Exception("File size must be less than 2MB");
            //file extension
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                throw new Exception("Only .jpg, .jpeg, and .png files are allowed");
            //  generate filename 
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            //  save file
            string path = Path.Combine(_env.WebRootPath, "uploads", "thumbnails");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            //path to save file
            string fullPath = Path.Combine(path, fileName);
            //Save  file to steam
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            // Return t path to store in database
            return "/uploads/thumbnails/" + fileName;
        }
        //save video 
        // Handle video logic
        public string HandleVideo(IFormFile? file, string? youtubeUrl, string? videoType)
        {
            // ... (rest of handle video logic)
            var normalizedVideoType = (videoType ?? string.Empty).Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalizedVideoType))
            {
                normalizedVideoType = !string.IsNullOrWhiteSpace(youtubeUrl) ? "youtube" : "upload";
            }
            if (normalizedVideoType == "youtube")
            {
                if (string.IsNullOrWhiteSpace(youtubeUrl))
                {
                    throw new Exception("Please provide a YouTube intro video link.");
                }
                if (!Uri.TryCreate(youtubeUrl.Trim(), UriKind.Absolute, out var parsedUri))
                    throw new Exception("Invalid YouTube URL.");
                var host = parsedUri.Host.ToLowerInvariant();
                if (!host.Contains("youtube.com") && !host.Contains("youtu.be"))
                    throw new Exception("Only YouTube links are allowed for this option.");
                return youtubeUrl.Trim();
            }
            if (normalizedVideoType == "upload")
            {
                if (file == null || file.Length == 0)
                    throw new Exception("Please upload an intro video file.");
                long maxSize = 50 * 1024 * 1024; // 50MB
                var allowedExtensions = new[] { ".mp4", ".webm", ".mov" };
                // size check
                if (file.Length > maxSize)
                    throw new Exception("Video must be less than 50MB");
                // extension check
                string ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                    throw new Exception("Only mp4, webm, mov allowed");
                // unique filename
                string fileName = Guid.NewGuid().ToString() + ext;
                // path
                string path = Path.Combine(_env.WebRootPath, "uploads", "videos");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string fullPath = Path.Combine(path, fileName);
                // save file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                // return saved path
                return "/uploads/videos/" + fileName;
            }
            throw new Exception("Please choose a valid video source.");
        }

        // Upload Resume logic
        public string? UploadResume(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            // limit size to 5MB and allow only PDF
            long maxSize = 5 * 1024 * 1024;
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".pdf")
                throw new Exception("Only PDF resumes are allowed.");
            
            if (file.Length > maxSize)
                throw new Exception("Resume file must be less than 5MB.");

            string fileName = Guid.NewGuid().ToString() + extension;
            string path = Path.Combine(_env.WebRootPath, "uploads", "resumes");
            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = Path.Combine(path, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/uploads/resumes/" + fileName;
        }

        // Save Profile Photo
        public string? SaveProfilePhoto(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
                throw new Exception("Only images (.jpg, .jpeg, .png) are allowed.");

            string fileName = Guid.NewGuid().ToString() + extension;
            string path = Path.Combine(_env.WebRootPath, "images", "profiles");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = Path.Combine(path, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/images/profiles/" + fileName;
        }
    }
}
