using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class TagModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; } = null!;

        [Required]
        public int ProjectId { get; set; }

        public ICollection<int> TasksIds { get; set; } = new List<int>();
    }
}
