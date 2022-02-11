using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class UserOnProjectModel
    {
        [Required]
        public bool IsManager { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
