using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace WebAPI.TokenProviders
{
    public class CustomPasswordResetTokenProvider<TUser>
        : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CustomPasswordResetTokenProvider(IDataProtectionProvider dataProtectionProvider,
                                        IOptions<CustomPasswordResetTokenProviderOptions> options,
                                        ILogger<CustomPasswordResetTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        { }
    }
}
