using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public enum TaskStatuses : short
    {
        ToDo = 0,
        InProgress = 1,
        Validate = 2,
        Complete = 3,
        Archived = 4
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

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(150)")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset? Deadline { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        [Required]
        public TaskStatuses Status { get; set; }

        public TaskPriorities? Priority { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }


        public ICollection<Message> Messages { get; set; } = new List<Message>();

        public ICollection<File> Files { get; set; } = new List<File>();

        public ICollection<TaskExecutor> Executors { get; set; } = new List<TaskExecutor>();
    }
}
