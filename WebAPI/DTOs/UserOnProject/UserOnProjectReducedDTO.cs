namespace WebAPI.DTOs.UserOnProject
{
    public record UserOnProjectReducedDTO
    {
        public int UserId { get; init; }

        public string Role { get; init; } = string.Empty;
    }
}
