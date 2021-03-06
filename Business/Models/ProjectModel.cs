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
        public DateTime StartDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxToDo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxInProgress { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxValidate { get; set; }

        public Guid? InviteCode { get; set; }

        public bool IsInviteCodeActive { get; set; }

        public ICollection<int> ReleasesIds { get; set; } = new List<int>();
        public ICollection<(int ProjectId, int UserId)> TeamMembersIds { get; set; } = new List<(int, int)>();
        public ICollection<int> TasksIds { get; set; } = new List<int>();
    }
}
