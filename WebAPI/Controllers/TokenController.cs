using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _service;

        private readonly IRefreshTokenService _refreshTokenService;

        public TokenController(ITokenService tokenService, IRefreshTokenService refreshTokenService)
        {
            _service = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDTO tokenApiModel)
        {
            if (tokenApiModel is null)
                return BadRequest("Invalid client request");
            string accessToken = tokenApiModel.AccessToken;
            string refreshToken = tokenApiModel.RefreshToken;
            var principal = _service.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.GetId();
            if (userId == null)
                return BadRequest("Invalid client request");
            var tokenModel = await _refreshTokenService.FindAsync(tokenApiModel.RefreshToken, userId.Value);
            if (tokenModel is null || tokenModel.Token != refreshToken || tokenModel.ExpiryTime <= DateTime.UtcNow)
                return BadRequest("Invalid client request");
            var newAccessToken = _service.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _service.GenerateRefreshToken();
            tokenModel.Token = newRefreshToken;
            await _refreshTokenService.UpdateAsync(tokenModel);
            return new ObjectResult(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        [HttpPost, Authorize]
        [Route("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenDTO dto)
        {
            var userId = User.GetId();
            if (userId == null)
                return BadRequest("Invalid client request");
            var tokenModel = await _refreshTokenService.FindAsync(dto.Token, userId.Value);
            if (tokenModel is null)
                return NotFound();
            await _refreshTokenService.DeleteByIdAsync(tokenModel.Id);
            return NoContent();
        }
    }
}
