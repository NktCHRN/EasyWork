using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class ProjectModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [StringLength(4, MinimumLength = 3)]
        public string? MainPictureFormat { get; set; }

        public int? MaxToDo { get; set; }

        public int? MaxInProgress { get; set; }

        public int? MaxValidate { get; set; }

        [Required]
        public int OwnerId { get; set; }

        public ICollection<int>? ReleasesIds { get; set; }
        public ICollection<int>? TeamMembersIds { get; set; }
        public ICollection<int>? TasksIds { get; set; }
    }
}
