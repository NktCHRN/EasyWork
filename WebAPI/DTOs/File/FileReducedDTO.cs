namespace WebAPI.DTOs.File
{
    public record FileReducedDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public bool IsFull { get; set; }
    }
}
