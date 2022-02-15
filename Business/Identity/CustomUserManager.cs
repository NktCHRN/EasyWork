using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Business.Identity
{
    public class CustomUserManager : UserManager<User>
    {
        public CustomUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        /// <inheritdoc />
        /// <remarks>Also, includes phone number and custom properties validation</remarks>
        public override async Task<IdentityResult> CreateAsync(User user)
        {
            var phoneNumberValidation = ValidatePhoneNumber(user);
            if (!phoneNumberValidation.Succeeded)
                return phoneNumberValidation;
            var propertiesValidation = ValidateCustomProperties(user);
            if (!propertiesValidation.Succeeded)
                return propertiesValidation;
            return await base.CreateAsync(user);
        }

        /// <inheritdoc />
        /// <remarks>Also, includes phone number and custom properties validation</remarks>
        public override async Task<IdentityResult> UpdateAsync(User user)
        {
            var phoneNumberValidation = ValidatePhoneNumber(user);
            if (!phoneNumberValidation.Succeeded)
                return phoneNumberValidation;
            var propertiesValidation = ValidateCustomProperties(user);
            if (!propertiesValidation.Succeeded)
                return propertiesValidation;
            return await base.UpdateAsync(user);
        }

        /// <summary>
        /// Validates world phone numbers
        /// </summary>
        /// <param name="user">Identity user</param>
        /// <example>
        /// Valid phone numbers:
        /// 12345678901
        /// 123456789011234
        /// 380689789456
        /// 67712345
        /// Invalid phone numbers:
        /// +14167129018
        /// +38(068)2337711
        /// 1-234-567-8901 ext1234
        /// 1 (234) 567-8901
        /// 1.234.567.8901
        /// 1/234/567/8901
        /// </example>
        /// <returns>IdentityResult.Failed if:
        /// - the length is lower that 8 or bigger than 15
        /// - contains not only digits
        /// - phoneNumber is already taken
        /// Else, returns IdentityResult.Success
        /// </returns>
        public IdentityResult ValidatePhoneNumber(User user)
        {
            if (user.PhoneNumber is not null)
            {
                if (user.PhoneNumber.Length < 8)
                    return IdentityResult.Failed(new IdentityError() { Description = "This phone number is too short" });
                if (user.PhoneNumber.Length > 15)
                    return IdentityResult.Failed(new IdentityError() { Description = "This phone number is too long" });
                if (user.PhoneNumber.Any(c => !char.IsDigit(c)))
                    return IdentityResult.Failed(new IdentityError() { Description = "Phone number should contain only numbers" });
                if (Users.Where(u => u.Id != user.Id).Select(u => u.PhoneNumber).Contains(user.PhoneNumber))
                    return IdentityResult.Failed(new IdentityError() { Description = "This phone number is already taken" });
            }
            return IdentityResult.Success;
        }

        /// <summary>
        /// Validates custom properties according to the data annotation
        /// </summary>
        /// <param name="user">Identity user</param>
        /// <returns>IdentityResult.Failed with an error description if the user object is not valid, 
        /// IdentityResult.Success if it is valid</returns>
        public static IdentityResult ValidateCustomProperties(User user)
        {
            var results = new List<ValidationResult>();
            if(!Validator.TryValidateObject(user, new ValidationContext(user), results, true))
                return IdentityResult.Failed(new IdentityError() { Description = results.Select(o => o.ErrorMessage).FirstOrDefault() });
            return IdentityResult.Success;
        }
    }
}
