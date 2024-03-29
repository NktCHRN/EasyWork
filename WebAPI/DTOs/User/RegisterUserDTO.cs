﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.DTOs.User
{
    public record RegisterUserDTO
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, ErrorMessage = "The maximum length of the first name is 50")]
        public string FirstName { get; init; } = string.Empty;

        [Column(TypeName = "nvarchar(50)")]
        [StringLength(50, ErrorMessage = "The maximum length of the last name is 50")]
        public string? LastName { get; init; }

        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string Password { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string PasswordConfirm { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string ClientURI { get; init; } = string.Empty;
    }
}
