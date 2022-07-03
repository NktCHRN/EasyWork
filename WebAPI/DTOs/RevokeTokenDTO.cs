namespace WebAPI.DTOs
{
    public record RevokeTokenDTO
    {
        public string Token { get; init; } = string.Empty;
    }
}
