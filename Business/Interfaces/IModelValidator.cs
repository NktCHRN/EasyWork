namespace Business.Interfaces
{
    public interface IModelValidator<TModel> where TModel : class
    {
        bool IsValid(TModel model, out string? firstErrorMessage);
    }
}
