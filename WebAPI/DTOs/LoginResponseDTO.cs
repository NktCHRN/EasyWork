namespace WebAPI.DTOs
{
    public record LoginResponseDTO
    {
        public bool IsAuthSuccessful { get; init; }
        public string? ErrorMessage { get; init; }
        public object? ErrorDetails { get; init; }
        public TokenDTO? Token { get; init; }
    }
}
