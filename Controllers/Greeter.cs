using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoVision_Backend_tasks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GreeterController : ControllerBase
    {
        [HttpGet]
        public IActionResult Greet([FromQuery] string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "anonymous";
           
            return Ok($"Hello {name}");
        }
    }
}
