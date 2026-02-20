//Task 45
using AgeCalculator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoVision_Backend_tasks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BirthDateController : ControllerBase
    {
        [HttpGet]
        public IActionResult Calculate(
            [FromQuery] string? name,
            [FromQuery] int? years,
            [FromQuery] int? months,
            [FromQuery] int? days
            )
        {
            string message = string.Empty;
            name = string.IsNullOrWhiteSpace(name) ? "anonymous" : name;

            if (years == null || months == null || days == null) return Ok("Hello " + name + ", I can’t calculate your age without knowing your birthdate!");
            try
            {
                var userInput = new DateTime((int)years, (int)months, (int)days);
                var age = new Age(userInput, DateTime.Today);
                message = "Hello " + name + ", your age is: " + age.Years;
            }
            catch
            {
                return BadRequest("there is error in the dates");
            }

            return Ok(message);
        }
    }
}