﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(150)")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "nvarchar(10)")]
        public string? MainPictureFormat { get; set; }

        public int? MaxToDo { get; set; }

        public int? MaxInProgress { get; set; }

        public int? MaxValidate { get; set; }

        public Guid? InviteCode { get; set; }

        public bool IsInviteCodeActive { get; set; }

        [Required]
        public int OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public User? Owner { get; set; }


        public ICollection<Release> Releases { get; set; } = new List<Release>();
        public ICollection<UserOnProject> TeamMembers { get; set; } = new List<UserOnProject>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
