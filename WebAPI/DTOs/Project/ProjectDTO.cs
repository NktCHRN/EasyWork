namespace WebAPI.DTOs.Project
{
    public record ProjectDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public DateTimeOffset StartDate { get; init; }

        public Guid? InviteCode { get; init; }

        public bool IsInviteCodeActive { get; init; }

    }
}
