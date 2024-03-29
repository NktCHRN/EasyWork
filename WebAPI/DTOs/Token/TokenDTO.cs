﻿namespace WebAPI.DTOs.Token
{
    public record TokenDTO
    {
        public string AccessToken { get; init; } = string.Empty;

        public string RefreshToken { get; init; } = string.Empty;
    }
}
