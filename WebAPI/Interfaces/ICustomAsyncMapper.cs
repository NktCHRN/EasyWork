namespace WebAPI.Interfaces
{
    public interface ICustomAsyncMapper<in T, K>
    {
        Task<K?> MapAsync(T? model);
    }
}
