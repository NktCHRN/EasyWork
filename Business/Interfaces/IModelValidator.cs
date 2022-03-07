using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Interfaces
{
    public interface IModelValidator<TModel> where TModel : class
    {
        bool IsValid(TModel model, out string? firstErrorMessage);
    }
}
