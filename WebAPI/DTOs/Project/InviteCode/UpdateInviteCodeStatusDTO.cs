using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Project.InviteCode
{
    public record UpdateInviteCodeStatusDTO
    {
        [Required]
        public bool? IsInviteCodeActive { get; init; }
    }
}
