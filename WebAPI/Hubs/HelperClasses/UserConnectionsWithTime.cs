namespace WebAPI.Hubs.HelperClasses
{
    public class UserConnectionsWithTime
    {
        public ICollection<string> Connections { get; set; } = new List<string>();

        public DateTimeOffset Time { get; set; } = DateTimeOffset.Now;
    }
}
