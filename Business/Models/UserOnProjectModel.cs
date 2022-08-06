using Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class UserOnProjectModel
    {
        [Required]
        public UserOnProjectRoles Role { get; set; }

        public DateTimeOffset AdditionDate { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
