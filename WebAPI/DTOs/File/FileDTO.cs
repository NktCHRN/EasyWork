namespace WebAPI.DTOs.File
{
    public record FileDTO : FileReducedDTO
    {
        public long Size { get; init; }
    }
}
