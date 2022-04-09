using Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskStatusesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(Enum.GetNames(typeof(TaskStatuses)));
    }
}
