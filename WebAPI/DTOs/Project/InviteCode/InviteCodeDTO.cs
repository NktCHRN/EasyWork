namespace WebAPI.DTOs.Project.InviteCode
{
    public record InviteCodeDTO
    {
        public Guid? InviteCode { get; init; }

        public bool IsInviteCodeActive { get; init; }
    }
}
