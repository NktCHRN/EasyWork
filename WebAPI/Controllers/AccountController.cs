using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly ITokenService _tokenService;

        private readonly IBanService _banService;

        private readonly IMapper _mapper;

        private readonly IFileManager _fileManager;

        private readonly IMailService _mailService;

        public AccountController(UserManager<User> userManager, ITokenService tokenService, IMapper mapper, IBanService banService, IFileManager fileManager, IMailService mailService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _banService = banService;
            _fileManager = fileManager;
            _mailService = mailService;
        }

        private async Task<IEnumerable<Claim>> GetClaimsAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Email, user.Email)
                };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
            return claims;
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
            if (!(await _userManager.CheckPasswordAsync(user, model.Password)))
                return Unauthorized(new LoginResponseDTO()
                {
                    ErrorMessage = "Wrong email or password"
                });
            if (!user.EmailConfirmed)
                return Unauthorized(new LoginResponseDTO()
                {
                    ErrorMessage = "Please, confirm your email first"
                });
            var bans = _banService.GetActiveUserBans(user.Id);
            if (bans.Any())
                return Unauthorized(new LoginResponseDTO()
                {
                    ErrorMessage = "You are banned from this website",
                    ErrorDetails = _mapper.Map<IEnumerable<BannedUserDTO>>(bans)
                });
            user.LastSeen = DateTime.Now;
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _userManager.UpdateAsync(user);
            return Ok(new LoginResponseDTO()
            {
                IsAuthSuccessful = true,
                Token = new TokenDTO()
                {
                    AccessToken = _tokenService.GenerateAccessToken(await GetClaimsAsync(user)),
                    RefreshToken = refreshToken
                }
            });
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (model.Password != model.PasswordConfirm)
                return BadRequest("Passwords are different");
            var user = _mapper.Map<User>(model);
            user.RegistrationDate = DateTime.Now;
            user.UserName = model.Email;
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser is not null 
                && !existingUser.EmailConfirmed 
                && existingUser.RegistrationDate.AddDays(1) < DateTime.Now)
                await _userManager.DeleteAsync(existingUser);
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            else
            {
                await _userManager.AddToRoleAsync(user, "User");
                string data = System.IO.File.ReadAllText(Path.Combine(_fileManager.GetSolutionPath()!, "WebAPI\\Mails\\ConfirmEmail.html"));
                var doc = new HtmlDocument();
                doc.LoadHtml(data);
                var logo = doc.GetElementbyId("logo");
                logo.Attributes.Add("src", $"data:image/png;base64, {Convert.ToBase64String(await _fileManager.GetFileContentAsync("logo.png", Business.Enums.EasyWorkFileTypes.EasyWorkProjectImage))}");
                var link = doc.GetElementbyId("link");
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var param = new Dictionary<string, string?>
                {
                    {"token", token },
                    {"email", user.Email }
                };
                var callback = QueryHelpers.AddQueryString(model.ClientURI, param);

                link.Attributes.Add("href", callback);
                await _mailService.SendAsync(new Business.Other.MailRequest()
                {
                    To = model.Email,
                    Subject = "EasyWork: Confirm your email",
                    Body = doc.DocumentNode.InnerHtml
                });
                return StatusCode(201);
            }
        }

        [HttpGet("EmailConfirmation")]
        public async Task<IActionResult> EmailConfirmation([FromQuery] string email, [FromQuery] string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return BadRequest("Invalid Email Confirmation Request");
            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
            if (!confirmResult.Succeeded)
                return BadRequest("Invalid Email Confirmation Request");
            return Ok();
        }
    }
}
