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
    public class Task
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(150)")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public TaskStatuses Status { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int? ExecutorId { get; set; }
        [ForeignKey("ExecutorId")]
        public User? Executor { get; set; }


        public ICollection<Message>? Messages { get; set; }

        public ICollection<File>? Files { get; set; }
    }
}
