namespace WebAPI.DTOs.Task.Status
{
    public record StatusChangeDTO
    {
        public int Id { get; init; }

        /// <summary>
        /// An old status
        /// </summary>
        public string Old { get; init; } = string.Empty;

        /// <summary>
        /// A new status
        /// </summary>
        public string New { get; init; } = string.Empty;
    }
}
