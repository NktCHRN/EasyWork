using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public string Name { get; set; } = null!;

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public ICollection<Task> Tasks { get; set; } = null!;
    }
}
