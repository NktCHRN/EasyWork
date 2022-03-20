using System.ComponentModel.DataAnnotations;

namespace Business.Interfaces
{
    public interface IModelValidator<TModel> where TModel : class
    {
        bool IsValid(TModel model, out string? firstErrorMessage);

        protected static bool IsValidByDefault(TModel model, out string? firstErrorMessage)
        {
            firstErrorMessage = null;
            if (model is null)
            {
                firstErrorMessage = "Model cannot be null";
                return false;
            }
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);
            if (!isValid)
            {
                firstErrorMessage = validationResults.First().ErrorMessage;
                return false;
            }
            return true;
        }
    }
}
