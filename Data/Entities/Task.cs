using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public enum TaskStatuses : short
    {
        ToDo = 0,
        InProgress = 1,
        Validate = 2,
        ReturnedToExecutor = 3,
        Complete = 4,
        Archived = 5
    }
    public enum TaskPriorities : short
    {
        Lowest = -2,
        Low = -1,
        Middle = 0,
        High = 1,
        Highest = 2
    }
    public class Task
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(150)")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? Deadline { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public TaskStatuses Status { get; set; }

        public TaskPriorities? Priority { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int? ExecutorId { get; set; }
        [ForeignKey("ExecutorId")]
        public User? Executor { get; set; }


        public ICollection<Message> Messages { get; set; } = new List<Message>();

        public ICollection<File> Files { get; set; } = new List<File>();

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
