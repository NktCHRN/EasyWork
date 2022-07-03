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
        [Column(TypeName = "nvarchar(50)")]
        [StringLength(50, ErrorMessage = "The maximum length of the last name is 50")]
        public string? LastName { get; set; } 

        [PersonalData]
        public DateTime RegistrationDate { get; set; }

        [Column(TypeName = "nvarchar(10)")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "The length of the avatar format should be 3 or 4")]
        public string? AvatarFormat { get; set; }

        public DateTime LastSeen { get; set; }

        public ICollection<Ban> Bans { get; set; } = new List<Ban>();
        public ICollection<Ban> GivenBans { get; set; } = new List<Ban>();

        public ICollection<UserOnProject> Projects { get; set; } = new List<UserOnProject>();

        public ICollection<Task> Tasks { get; set; } = new List<Task>();

        public ICollection<Message> Messages { get; set; } = new List<Message>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
