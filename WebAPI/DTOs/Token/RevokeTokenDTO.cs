namespace WebAPI.DTOs.Token
{
    public record RevokeTokenDTO
    {
        public string Token { get; init; } = string.Empty;
    }
}
