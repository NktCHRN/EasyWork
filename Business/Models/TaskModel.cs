using Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? Deadline { get; set; }

        public DateTime? EndDate { get; set; }      // should be null on creation

        [Required]
        public TaskStatuses Status { get; set; }

        public TaskPriorities? Priority { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public int? ExecutorId { get; set; }        // may be or not be null on creation


        public ICollection<int> Messages { get; set; } = null!;

        public ICollection<int> Files { get; set; } = null!;

        public ICollection<int> TagsIds { get; set; } = null!;
    }
}
