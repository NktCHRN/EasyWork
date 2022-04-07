using Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskPrioritiesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var values = ((TaskPriorities[])Enum.GetValues(typeof(TaskPriorities)))
                .OrderBy(t => (short)t);
            var names = values
                .Select(t => t.ToString());
            return Ok(names);
        }
    }
}
