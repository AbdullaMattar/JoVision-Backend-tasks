using Microsoft.AspNetCore.Http;

namespace JoVision_Backend_tasks.Models
{
    public class ImageUploadRequest
    {
        public IFormFile File { get; set; }

        public string Owner { get; set; }
    }

    public class ImageMetadata
    {
        public string Owner { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public DateTime LastModificationTime { get; set; }
    }
}