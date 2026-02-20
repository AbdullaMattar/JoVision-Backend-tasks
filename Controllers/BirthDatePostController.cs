//Task 46
using AgeCalculator;
using Microsoft.AspNetCore.Mvc;

namespace JoVision_Backend_tasks.Controllers
{
    public class AgeCalculationRequest
    {
        public string? Name { get; set; }
        public int? Years { get; set; }
        public int? Months { get; set; }
        public int? Days { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class BirthDatePostController : ControllerBase
    {
        [HttpPost] // Changed from HttpGet
        public IActionResult Calculate([FromForm] AgeCalculationRequest request) //FromBody can also be used and it accept json data, but I used FromForm to accept form data
        {
            string? name = request.Name;
            int? years = request.Years;
            int? months = request.Months;
            int? days = request.Days;

            //same logic as before
            string message = string.Empty;
            name = string.IsNullOrWhiteSpace(name) ? "anonymous" : name;

            if (years == null || months == null || days == null) return Ok($"Hello {name}, I can’t calculate your age without knowing your birthdate!");
            try
            {
                var userInput = new DateTime(years.Value, months.Value, days.Value);
                var age = new Age(userInput, DateTime.Today);
                message = $"Hello {name}, your age is: {age.Years}";
            }
            catch
            {
                return BadRequest("there is error in the dates");
            }

            return Ok(message);
        }
    }
}