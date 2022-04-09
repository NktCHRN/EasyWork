﻿using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record AddProjectDTO
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(150)]
        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }
    }
}
