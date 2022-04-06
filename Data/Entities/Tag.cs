using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(20)")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}
