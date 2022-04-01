using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using WebAPI.Other;
using Data.Entities;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _service;

        private readonly IFileManager _manager;

        private readonly IMessageService _messageService;

        private readonly ITaskService _taskService;

        private readonly IUserOnProjectService _uopService;

        public FilesController(IFileService service, IFileManager manager, IMessageService messageService, 
            ITaskService taskService, IUserOnProjectService uopService)
        {
            _service = service;
            _manager = manager;
            _messageService = messageService;
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

            int taskId;
            if (model.MessageId is not null)
            {
                var message = await _messageService.GetByIdAsync(model.MessageId.GetValueOrDefault());
                if (message is null)
                    return NotFound();
                taskId = message.TaskId;
            }
            else if (model.TaskId is not null)
                taskId = model.TaskId.GetValueOrDefault();
            else
                return Forbid();

            var task = await _taskService.GetByIdAsync(taskId);
            if (task is null)
                return NotFound();
            if (task.ExecutorId != User.GetId())
            {
                    var role = await _uopService.GetRoleOnProjectAsync(task.ProjectId, User.GetId().GetValueOrDefault());
                    if (role is null || role < UserOnProjectRoles.Manager)
                        return Forbid();
            }

            var visualFileName = model.Name;
            var realFileName = model.Id + Path.GetExtension(visualFileName);
            var isSuccessful = (new FileExtensionContentTypeProvider()).TryGetContentType(visualFileName, out string? temp);
            string contentType;
            if (isSuccessful)
                contentType = temp!.ToString();
            else
                contentType = "application/octet-stream";
            return File(await _manager.GetFileContentAsync(realFileName, Business.Enums.EasyWorkFileTypes.File), contentType, visualFileName);
        }
    }
}
