using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class File
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string Name { get; set; } = null!;

        public int? TaskId { get; set; }
        public Task? Task { get; set; }

        public int? MessageId { get; set; }
        public Message? Message { get; set; }
    }
}
