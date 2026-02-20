using Microsoft.AspNetCore.Mvc;
using JoVision_Backend_tasks.Models;
using System.IO;
using System.Text.Json;

namespace JoVision_Backend_tasks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

        [HttpPost("Create")]
        public IActionResult CreateFile([FromForm] ImageUploadRequest request)
        {
            IFormFile? file = request.File;
            string? owner = request.Owner;

            // 1.Create the UploadedFiles directory if it doesn't exist
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            // 2.Check  the file is null or empty
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            // 3. ensure file extension is ".jpg".
            if (Path.GetExtension(file.FileName).ToLower() != ".jpg")
            {
                return BadRequest("Only .jpg files are allowed.");
            }

            // 4. Check if a file with this name already exists in _storagePath.
            var fullPathToFile = Path.Combine(_storagePath, file.FileName);
            if (System.IO.File.Exists(fullPathToFile))
            {
                return BadRequest($"File {fullPathToFile} is already exist");
            }

            // 5. Save the file.
            try { 
            using (var stream = new FileStream(fullPathToFile, FileMode.Create))
                file.CopyTo(stream);
            } catch { return BadRequest("Error happened while saving the file"); }
            // 6. Create the metadata object
            var metadata = new ImageMetadata
            {
                Owner = owner ,
                CreationTime = DateTime.UtcNow,
                LastModificationTime = DateTime.UtcNow
            };

            // 7. Convert to a JSON string
            try { 
            var json = JsonSerializer.Serialize(metadata);

            // 8. Define the path for the JSON file.
            var jsonPath = Path.ChangeExtension(fullPathToFile, ".json");

            // 9. Write the string to the file system.
            System.IO.File.WriteAllText(jsonPath, json);
            }
            catch { return BadRequest("Error happened while saving the json"); }
            return StatusCode(201, "Image and metadata saved successfully.");
        }
    }
}