namespace WebAPI.DTOs
{
    public record MessageDTO
    {
        public int Id { get; init; }

        public string Text { get; init; } = string.Empty;

        public DateTime Date { get; init; }

        public UserMiniWithAvatarDTO Sender { get; init; } = new UserMiniWithAvatarDTO();
    }
}
