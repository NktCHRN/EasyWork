using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.UserOnProject
{
    public record UserOnProjectMiniDTO
    {
        [Required]
        public int ProjectId { get; init; }

        [Required]
        public int UserId { get; init; }
    }
}
