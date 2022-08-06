namespace WebAPI.DTOs.GeneralInfo
{
    public record GeneralInfoDTO
    {
        public int UsersCount { get; init; }

        public int ProjectsCount { get; init; }
    }
}
