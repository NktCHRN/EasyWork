using Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Business.Identity
{
    public class CustomPropertiesUserValidator : IUserValidator<User>
    {
        /// <summary>
        /// Validates custom properties according to the data annotation
        /// </summary>
        /// <param name="user">Identity user</param>
        /// <returns>IdentityResult.Failed with an error description if the user object is not valid, 
        /// IdentityResult.Success if it is valid</returns>
        public async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(user, new ValidationContext(user), results, true))
                    return IdentityResult.Failed(new IdentityError() { Description = results.Select(o => o.ErrorMessage).FirstOrDefault() });
                return IdentityResult.Success;
            }
            );
        }
    }
}
