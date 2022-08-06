using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Ban
{
    public record AddBanDTO
    {
        [Required]
        public DateTimeOffset DateTo { get; init; }

        [StringLength(400)]
        public string? Hammer { get; init; }

        [Required]
        public int UserId { get; init; }
    }
}
