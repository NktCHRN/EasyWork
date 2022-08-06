using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class File
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(256)")]
        public string Name { get; set; } = string.Empty;

        public bool IsFull { get; set; }

        [Required]
        public int TaskId { get; set; }
        public Task? Task { get; set; }
    }
}
