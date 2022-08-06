using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class TaskExecutor
    {
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }      // composite unique constraint
        public Task? Task { get; set; }

        [Required]
        public int UserId { get; set; }         // composite unique constraint
        public User? User { get; set; }
    }
}
