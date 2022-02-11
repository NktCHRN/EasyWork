using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class UserModel : IdentityUser<int>
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public DateTime RegistrationDate { get; set; }

        [StringLength(4, MinimumLength = 3)]
        public string? AvatarFormat { get; set; }

        public DateTime LastSeen { get; set; }

        public ICollection<int> BansIds { get; set; } = null!;
        public ICollection<int> GivenBansIds { get; set; } = null!;

        public ICollection<int> OwnedProjectsIds { get; set; } = null!;

        public ICollection<(int ProjectId, int UserId)> ProjectsIds { get; set; } = null!;

        public ICollection<int> TasksIds { get; set; } = null!;

        public ICollection<int> MessagesIds { get; set; } = null!;
    }
}
