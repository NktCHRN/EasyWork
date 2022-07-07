using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public enum UserOnProjectRoles : ushort
    {
        User = 0,
        Manager = 1,
        Owner = 2
    }
    public class UserOnProject
    {
        [Required]
        public UserOnProjectRoles Role { get; set; }

        public DateTimeOffset AdditionDate { get; set; }

        [Required]
        public int ProjectId { get; set; }      // composite key
        public Project? Project { get; set; }

        [Required]
        public int UserId { get; set; }         // composite key
        public User? User { get; set; }
    }
}
