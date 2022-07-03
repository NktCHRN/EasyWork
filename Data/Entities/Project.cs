using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Project
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(150)")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        public int? MaxToDo { get; set; }

        public int? MaxInProgress { get; set; }

        public int? MaxValidate { get; set; }

        public Guid? InviteCode { get; set; }

        public bool IsInviteCodeActive { get; set; }

        public ICollection<Release> Releases { get; set; } = new List<Release>();
        public ICollection<UserOnProject> TeamMembers { get; set; } = new List<UserOnProject>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}
