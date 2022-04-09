using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Business.Identity
{
    public class PhoneNumberUserValidator : IUserValidator<User>
    {
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
        /// null (no phone number)
        /// Invalid phone numbers:
        /// +14167129018
        /// +38(068)2337711
        /// 1-234-567-8901 ext1234
        /// 1 (234) 567-8901
        /// 1.234.567.8901
        /// 1/234/567/8901
        /// (empty)
        /// </example>
        /// <returns>IdentityResult.Failed if:
        /// - the length is lower that 8 or bigger than 15
        /// - contains not only digits
        /// - phoneNumber is already taken
        /// Else, returns IdentityResult.Success
        /// </returns>
        public async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
        {
            if (user.PhoneNumber is not null)
            {
                if (user.PhoneNumber.Length < 8)
                    return IdentityResult.Failed(new IdentityError() { Description = "This phone number is too short" });
                if (user.PhoneNumber.Length > 15)
                    return IdentityResult.Failed(new IdentityError() { Description = "This phone number is too long" });
                if (await System.Threading.Tasks.Task.Run(() => user.PhoneNumber.Any(c => !char.IsDigit(c))))
                    return IdentityResult.Failed(new IdentityError() { Description = "Phone number should contain only numbers" });
            }
            return IdentityResult.Success;
        }
    }
}
