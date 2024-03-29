﻿using Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset? Deadline { get; set; }

        public DateTimeOffset? EndDate { get; set; }      // should be null on creation

        [Required]
        public TaskStatuses Status { get; set; }

        public TaskPriorities? Priority { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public ICollection<int> MessagesIds { get; set; } = new List<int>();

        public ICollection<int> FilesIds { get; set; } = new List<int>();

        public ICollection<int> ExecutorsIds { get; set; } = new List<int>();
    }
}
