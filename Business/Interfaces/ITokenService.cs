﻿using Google.Apis.Auth;
using System.Security.Claims;

namespace Business.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        public string GenerateRefreshToken();
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

        public Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken);
    }
}
