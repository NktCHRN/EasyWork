using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class RefreshTokenModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Token { get; set; } = string.Empty;

        public DateTimeOffset ExpiryTime { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
