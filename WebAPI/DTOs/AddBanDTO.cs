using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record AddBanDTO
    {
        [Required]
        public DateTime DateTo { get; init; }

        [StringLength(400)]
        public string? Hammer { get; init; }

        [Required]
        public int UserId { get; init; }
    }
}
