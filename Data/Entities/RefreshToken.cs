using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryTime { get; set; }

        [Required]
        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
