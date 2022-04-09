using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class TagModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(20)]
        public string Name { get; set; } = string.Empty;

        public ICollection<int> TasksIds { get; set; } = new List<int>();
    }
}
