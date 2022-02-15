using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class User : IdentityUser<int>
    {
        [PersonalData]
        [Required]
        [Column(TypeName = "nvarchar(50)")]
        [StringLength(50, ErrorMessage = "The maximum length of the first name is 50")]
        public string FirstName { get; set; } = null!;

        [PersonalData]
        [Required]
        [Column(TypeName = "nvarchar(50)")]
        [StringLength(50, ErrorMessage = "The maximum length of the last name is 50")]
        public string LastName { get; set; } = null!;

        [PersonalData]
        public DateTime RegistrationDate { get; set; }

        [Column(TypeName = "nvarchar(10)")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "The length of the avatar format should be 3 or 4")]
        public string? AvatarFormat { get; set; }

        public DateTime LastSeen { get; set; }

        public ICollection<Ban> Bans { get; set; } = null!;
        public ICollection<Ban> GivenBans { get; set; } = null!;

        public ICollection<Project> OwnedProjects { get; set; } = null!;

        public ICollection<UserOnProject> Projects { get; set; } = null!;

        public ICollection<Task> Tasks { get; set; } = null!;

        public ICollection<Message> Messages { get; set; } = null!;
    }
}
