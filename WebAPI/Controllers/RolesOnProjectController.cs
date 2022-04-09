using Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesOnProjectController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(Enum.GetNames(typeof(UserOnProjectRoles)));
    }
}
