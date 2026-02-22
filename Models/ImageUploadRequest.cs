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

    public enum FileFilterType
    {
        ByModificationDate,
        ByCreationDateDescending,
        ByCreationDateAscending,
        ByOwner
    }

    public class FilterRequest
    {
        public DateTime? CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? Owner { get; set; }
        public FileFilterType? FilterType { get; set; }
    }

    public class FilterResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
    }
}