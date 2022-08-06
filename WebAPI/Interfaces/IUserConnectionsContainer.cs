namespace WebAPI.Interfaces
{
    public interface IUserConnectionsContainer
    {
        public IDictionary<int, ICollection<string>> UserConnections { get; }
    }
}
