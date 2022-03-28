using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly IConfiguration _configuration;

        public AccountController(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody]LoginUserDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new LoginResponseDTO()
                {
                    ErrorMessage = "Invalid authentication request"
                });
            var user = await _userManager.FindByNameAsync(model.Email);
            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var now = DateTime.UtcNow;
                // создаем JWT-токен
                var jwt = new JwtSecurityToken(
                        issuer: _configuration.GetSection("JwtBearer:Issuer").Value,
                        audience: _configuration.GetSection("JwtBearer:Audience").Value,
                        notBefore: now,
                        claims: (await GetClaimsAsync(user)),
                        expires: now.Add(TimeSpan.FromMinutes(int.Parse(_configuration.GetSection("JwtBearer:Lifetime").Value))),
                        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("JwtBearer:Secret").Value)), SecurityAlgorithms.HmacSha256));
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                return Ok(new LoginResponseDTO()
                {
                    IsAuthSuccessful = true,
                    Token = encodedJwt
                });
            }
            return Unauthorized(new LoginResponseDTO()
            {
                ErrorMessage = "Wrong email or password"
            });
        }

        private async Task<IEnumerable<Claim>?> GetClaimsAsync(User? user)
        {
            if (user is not null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Email, user.Email)
                };
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
                return claims;
            }
            return null;
        }
    }
}
