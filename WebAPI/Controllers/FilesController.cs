using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Other;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using WebAPI.DTOs.File;
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

        private readonly IMapper _mapper;

        public FilesController(IFileService service, IFileManager manager,
            ITaskService taskService, IUserOnProjectService uopService, IMapper mapper)
        {
            _service = service;
            _manager = manager;
            _taskService = taskService;
            _uopService = uopService;
            _mapper = mapper;
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
            if (!await _uopService.IsOnProjectAsync(task.ProjectId, User.GetId().GetValueOrDefault()))
                    return Forbid();

            var visualFileName = model.Name;
            var realFileName = model.Id + Path.GetExtension(visualFileName);
            var isSuccessful = new FileExtensionContentTypeProvider().TryGetContentType(visualFileName, out string? temp);
            string contentType;
            if (isSuccessful)
                contentType = temp!.ToString();
            else
                contentType = "application/octet-stream";
            byte[] content;
            try
            {
                content = await _manager.GetFileContentAsync(realFileName, Business.Enums.EasyWorkFileTypes.File);
            }
            catch (FileNotFoundException)
            {
                return NotFound("Most likely, the file is not fully loaded or damaged");
            }
            return File(content, contentType, visualFileName);
        }

        [Route("{id}/chunk/{index}")]
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 102400)]      // 100 KB
        public async Task<IActionResult> UploadChunk(int id, IFormFile chunk, int index)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _service.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            var taskModel = await _taskService.GetByIdAsync(model.TaskId);
            if (taskModel is null)
                return NotFound();
            if (!await _uopService.IsOnProjectAsync(taskModel.ProjectId, userId.Value))
                return Forbid();
            try
            {
                await _service.AddChunkAsync(id, new FileChunkModel
                {
                    Index = index,
                    ChunkFile = chunk
                });
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [Route("{id}/end")]
        [HttpPost]
        public async Task<IActionResult> EndUpload(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _service.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            var taskModel = await _taskService.GetByIdAsync(model.TaskId);
            if (taskModel is null)
                return NotFound();
            if (!await _uopService.IsOnProjectAsync(taskModel.ProjectId, userId.Value))
                return Forbid();
            FileModelExtended result;
            try
            {
                result = await _service.ChunkAddEndAsync(id);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
            return Created($"{this.GetApiUrl()}Files/{id}", _mapper.Map<FileDTO>(result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null)
                return NotFound();

            var task = await _taskService.GetByIdAsync(model.TaskId);
            if (task is null)
                return NotFound();
            if (!await _uopService.IsOnProjectAsync(task.ProjectId, User.GetId().GetValueOrDefault()))
                return Forbid();

            try
            {
                await _service.DeleteByIdAsync(id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
