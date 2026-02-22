using JoVision_Backend_tasks.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using static System.Net.WebRequestMethods;

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

            return Ok("Image and metadata deleted successfully.");
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

        [HttpPost("Filter")]
        public IActionResult Filter([FromForm] FilterRequest request)
        {
            string? owner = request.Owner;
            FileFilterType? filterType = request.FilterType;
            DateTime? creationDate = request.CreationDate;
            DateTime? modificationDate = request.ModificationDate;

            //check for valid filter type
            if (filterType == null || !Enum.IsDefined(typeof(FileFilterType), filterType))
            {
                return BadRequest("Invalid or missing filter type.");
            }

            // 1. Create the UploadedFiles directory if it doesn't exist
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            // 2. Get all .json files in the directory
            var jsonFiles = Directory.GetFiles(_storagePath, "*.json");
            // get files metadata
            var filesMetadata = new List<(string FileName, ImageMetadata Metadata)>();

            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var jsonContent = System.IO.File.ReadAllText(jsonFile);
                    var metadata = JsonSerializer.Deserialize<ImageMetadata>(jsonContent);
                    if (metadata != null)
                    {
                        filesMetadata.Add((Path.GetFileNameWithoutExtension(jsonFile), metadata));
                    }
                }
                catch
                {
                    return BadRequest($"Error reading the json file {jsonFile}");
                }
            }
            // 3. Filter the files based on the request parameters
            IEnumerable<(string FileName, ImageMetadata Metadata)> filteredData = filesMetadata;

            switch (request.FilterType)
            {
                case FileFilterType.ByModificationDate:

                    if (request.ModificationDate == null) return BadRequest("ModificationDate is required for this filter.");

                    filteredData = filteredData.Where(x => x.Metadata.LastModificationTime < request.ModificationDate.Value);
                    break;

                case FileFilterType.ByCreationDateDescending:
                    if (request.CreationDate == null) return BadRequest("CreationDate is required for this filter.");
                    filteredData = filteredData.Where(x => x.Metadata.CreationTime > request.CreationDate.Value);
                    filteredData = filteredData.OrderByDescending(x => x.Metadata.CreationTime);
                    break;

                case FileFilterType.ByCreationDateAscending:
                    if (request.CreationDate == null) return BadRequest("CreationDate is required for this filter.");
                    filteredData = filteredData.Where(x => x.Metadata.CreationTime > request.CreationDate.Value);
                    filteredData = filteredData.OrderBy(x => x.Metadata.CreationTime);
                    break;

                case FileFilterType.ByOwner:
                    if (string.IsNullOrWhiteSpace(request.Owner)) return BadRequest("Owner is required for this filter.");
                    filteredData = filteredData.Where(x => x.Metadata.Owner == request.Owner);
                    break;

                default:
                    return BadRequest("FilterType is invalid");
            }
            // 4. Return the filtered list of files with their metadata
            var finalResult = filteredData.Select(x => new FilterResponse
            {
                FileName = x.FileName,
                OwnerName = x.Metadata.Owner
            }).ToList();

            return Ok(finalResult);
        }

        [HttpGet("TransferOwnership")]
        public IActionResult TransferOwnership([FromQuery] string? OldOwner, [FromQuery] string? NewOwner)
        {
            if (string.IsNullOrWhiteSpace(OldOwner) || string.IsNullOrWhiteSpace(NewOwner))
            { return BadRequest("NewOwner or OldOwner can't be empty"); }

            // read all files
            var jsonFiles = Directory.GetFiles(_storagePath, "*.json");
            // get files metadata
            var filesMetadata = new List<(string FileName, ImageMetadata Metadata)>();

            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var jsonContent = System.IO.File.ReadAllText(jsonFile);
                    var metadata = JsonSerializer.Deserialize<ImageMetadata>(jsonContent);
                    if (metadata != null)
                    {
                        filesMetadata.Add((Path.GetFileNameWithoutExtension(jsonFile), metadata));
                    }
                }
                catch
                {
                    return BadRequest($"Error reading the json file {jsonFile}");
                }
            }
            // check if old Owner Has Files
            var oldOwnerFiles = filesMetadata.Where(x => OldOwner == x.Metadata.Owner);

            if (oldOwnerFiles.Count() == 0) return NotFound($"old owner {OldOwner} has no files");

            var transferred = new List<ImageMetadata>();
            var failed = new List<string>();
            //change owner of old files
            foreach ((string FileName, ImageMetadata Metadata) item in oldOwnerFiles)
            {
                var jsonPath = Path.Combine(_storagePath, item.FileName + ".json");
                try
                {
                    item.Metadata.Owner = NewOwner;
                    item.Metadata.LastModificationTime = DateTime.UtcNow;

                    var json = JsonSerializer.Serialize(item.Metadata);
                    System.IO.File.WriteAllText(jsonPath, json);
                }
                catch
                {
                    item.Metadata.Owner = OldOwner;
                    return BadRequest($"Error adding {item.FileName}");
                }
            }
            // print the new owner files
            var finalResult = filesMetadata
                .Where(x => x.Metadata.Owner == NewOwner)
                .Select(x => new FilterResponse
                {
                    FileName = x.FileName,
                    OwnerName = x.Metadata.Owner
                }).ToList();

            return Ok(finalResult);
        }
    }
}