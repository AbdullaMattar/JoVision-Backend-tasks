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
            try
            {
                using (var stream = new FileStream(fullPathToFile, FileMode.Create))
                    file.CopyTo(stream);
            }
            catch { return BadRequest("Error happened while saving the file"); }
            // 6. Create the metadata object
            var metadata = new ImageMetadata
            {
                Owner = owner,
                CreationTime = DateTime.UtcNow,
                LastModificationTime = DateTime.UtcNow
            };

            // 7. Convert to a JSON string
            try
            {
                var json = JsonSerializer.Serialize(metadata);

                // 8. Define the path for the JSON file.
                var jsonPath = Path.ChangeExtension(fullPathToFile, ".json");

                // 9. Write the string to the file system.
                System.IO.File.WriteAllText(jsonPath, json);
            }
            catch { return BadRequest("Error happened while saving the json"); }
            return StatusCode(201, "Image and metadata saved successfully.");
        }

        [HttpGet("Delete")]
        public IActionResult DeleteFile([FromQuery] string? FileName, [FromQuery] string? FileOwner)
        {
            if (string.IsNullOrWhiteSpace(FileName) || string.IsNullOrWhiteSpace(FileOwner))
            { return BadRequest("File Name or Owner can't be empty"); }

            // Check if a file with this name already exists in _storagePath.
            var filePath = Path.Combine(_storagePath, FileName);
            var imagePath = filePath;

            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest($"File {imagePath} dosn't exist");
            }
            // check the owner
            var jsonPath = Path.ChangeExtension(imagePath, ".json");
            var jsonfile = "";
            ImageMetadata? jsonContent = null;
            try
            {
                jsonfile = System.IO.File.ReadAllText(jsonPath);
                jsonContent = JsonSerializer.Deserialize<ImageMetadata>(jsonfile);
            }
            catch
            {
                return BadRequest("Error reading the json file");
            }
            if (FileOwner != jsonContent.Owner)
            { return BadRequest("File Owner is Wrong"); }

            //delete the file
            try
            {
                System.IO.File.Delete(imagePath);
                System.IO.File.Delete(jsonPath);
            }
            catch
            {
                return BadRequest("Error happened while deleting the file");
            }

            return StatusCode(201, "Image and metadata deleted successfully.");
        }

        [HttpPost("Update")]
        public IActionResult UpdateFile([FromForm] ImageUploadRequest request)
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
            var safeFileName = Path.GetFileName(file.FileName);
            var fullPathToFile = Path.Combine(_storagePath, safeFileName);
            if (!System.IO.File.Exists(fullPathToFile))
            {
                return BadRequest($"File {fullPathToFile} dosn't exist");
            }

            // check the owner
            var jsonPath = Path.ChangeExtension(fullPathToFile, ".json");
            var jsonfile = "";
            ImageMetadata? jsonContent = null;
            try
            {
                jsonfile = System.IO.File.ReadAllText(jsonPath);
                jsonContent = JsonSerializer.Deserialize<ImageMetadata>(jsonfile);
            }
            catch
            {
                return BadRequest("Error reading the json file");
            }
            if (owner != jsonContent.Owner)
            { return StatusCode(403, "Forbidden: File Owner is Wrong"); }

            // 5. Update the file.
            try
            {
                using (var stream = new FileStream(fullPathToFile, FileMode.Create))
                    file.CopyTo(stream);
            }
            catch { return BadRequest("Error happened while saving the file"); }
            // 6. update the metadata object
            var metadata = new ImageMetadata
            {
                Owner = owner,
                CreationTime = jsonContent.CreationTime,
                LastModificationTime = DateTime.UtcNow
            };

            // 7. Convert to a JSON string
            try
            {
                var json = JsonSerializer.Serialize(metadata);

                // 8. Write the string to the file system.
                System.IO.File.WriteAllText(jsonPath, json);
            }
            catch { return BadRequest("Error happened while saving the json"); }
            return Ok("Image and metadata saved successfully.");
        }

        [HttpGet("Retrieve")]
        public IActionResult RetrieveFile([FromQuery] string? FileName, [FromQuery] string? FileOwner)
        {
            if (string.IsNullOrWhiteSpace(FileName) || string.IsNullOrWhiteSpace(FileOwner))
            { return BadRequest("File Name or Owner can't be empty"); }

            // Check if a file with this name already exists in _storagePath.

            var safeFileName = Path.GetFileName(FileName);
            var filePath = Path.Combine(_storagePath, safeFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"File {filePath} dosn't exist");
            }
            // check the owner
            var jsonPath = Path.ChangeExtension(filePath, ".json");
            var jsonfile = "";
            ImageMetadata? jsonContent = null;
            try
            {
                jsonfile = System.IO.File.ReadAllText(jsonPath);
                jsonContent = JsonSerializer.Deserialize<ImageMetadata>(jsonfile);
            }
            catch
            {
                return BadRequest("Error reading the json file");
            }
            if (FileOwner != jsonContent.Owner)
            { return StatusCode(403, "Forbidden: File Owner is Wrong"); }

            //Retrieve the file
            var image = new byte[0];
            try
            {
                image = System.IO.File.ReadAllBytes(filePath);
            }
            catch
            {
                return BadRequest("Error happened while reading the file");
            }

            return File(image, "img/jpeg", FileName);
        }
    }
}