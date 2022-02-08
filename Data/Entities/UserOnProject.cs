using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class UserOnProject
    {
        [Required]
        public bool IsManager { get; set; }

        [Required]
        public int ProjectId { get; set; }      // composite key
        public Project? Project { get; set; }

        [Required]
        public int UserId { get; set; }         // composite key
        public User? User { get; set; }
    }
}
