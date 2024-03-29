﻿using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class BanModel
    {
        public int Id { get; set; }

        [Required]
        public DateTimeOffset DateFrom { get; set; }

        [Required]
        public DateTimeOffset DateTo { get; set; }

        [StringLength(400)]
        public string? Hammer { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int? AdminId { get; set; }
    }
}
