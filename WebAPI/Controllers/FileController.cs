using Business.Interfaces;
using Business.Models;
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
    public class FileController : ControllerBase
    {
        private readonly IFileService _service;

        private readonly IFileManager _manager;

        private readonly IMessageService _messageService;

        private readonly ITaskService _taskService;

        private readonly IUserOnProjectService _uopService;

        public FileController(IFileService service, IFileManager manager, IMessageService messageService, 
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
            FileModel model;
            try
            {
                model = await _service.GetByIdAsync(id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            int taskId;
            if (model.MessageId is not null)
            {
                    var message = await _messageService.GetByIdAsync(model.MessageId.GetValueOrDefault());
                    taskId = message.TaskId;
            }
            else if (model.TaskId is not null)
                taskId = model.TaskId.GetValueOrDefault();
            else
                return Forbid();

            var task = await _taskService.GetByIdAsync(taskId);
            if (task.ExecutorId != User.GetId())
            {
                try
                {
                    var role = await _uopService.GetRoleOnProjectAsync(task.ProjectId, User.GetId().GetValueOrDefault());
                    if (role < UserOnProjectRoles.Manager)
                        return Forbid();
                }
                catch (InvalidOperationException)
                {
                    return Forbid();
                }
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
