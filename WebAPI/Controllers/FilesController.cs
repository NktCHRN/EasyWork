using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _service;

        private readonly IFileManager _manager;

        private readonly ITaskService _taskService;

        private readonly IUserOnProjectService _uopService;

        public FilesController(IFileService service, IFileManager manager,
            ITaskService taskService, IUserOnProjectService uopService)
        {
            _service = service;
            _manager = manager;
            _taskService = taskService;
            _uopService = uopService;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null)
                return NotFound();

            var task = await _taskService.GetByIdAsync(model.TaskId);
            if (task is null)
                return NotFound();
            if (task.ExecutorId != User.GetId())
            {
                if (!await _uopService.IsOnProjectAsync(task.ProjectId, User.GetId().GetValueOrDefault()))
                    return Forbid();
            }

            var visualFileName = model.Name;
            var realFileName = model.Id + Path.GetExtension(visualFileName);
            var isSuccessful = new FileExtensionContentTypeProvider().TryGetContentType(visualFileName, out string? temp);
            string contentType;
            if (isSuccessful)
                contentType = temp!.ToString();
            else
                contentType = "application/octet-stream";
            return File(await _manager.GetFileContentAsync(realFileName, Business.Enums.EasyWorkFileTypes.File), contentType, visualFileName);
        }
    }
}
