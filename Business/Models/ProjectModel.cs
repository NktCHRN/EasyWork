using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class ProjectModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        [Required]
        public ProjectLimitsModel Limits { get; set; } = new ProjectLimitsModel();

        public Guid? InviteCode { get; set; }

        public bool IsInviteCodeActive { get; set; }

        public ICollection<int> ReleasesIds { get; set; } = new List<int>();
        public ICollection<(int ProjectId, int UserId)> TeamMembersIds { get; set; } = new List<(int, int)>();
        public ICollection<int> TasksIds { get; set; } = new List<int>();
    }
}
