using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Google.Apis.Auth;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Other;

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

        private readonly IConfiguration _configuration;

        private readonly IUserAvatarService _userAvatarService;

        private int EmailConfirmationLifeTime => int.Parse(_configuration.GetSection("TokenProvidersSetting:EmailConfirmationLifetime").Value);

        public AccountController(UserManager<User> userManager, ITokenService tokenService, IMapper mapper, IBanService banService, IFileManager fileManager, IMailService mailService, IConfiguration configuration, IUserAvatarService userAvatarService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _banService = banService;
            _fileManager = fileManager;
            _mailService = mailService;
            _configuration = configuration;
            _userAvatarService = userAvatarService;
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
            var user = _mapper.Map<User>(model);
            user.RegistrationDate = DateTime.Now;
            user.UserName = model.Email;
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser is not null 
                && !existingUser.EmailConfirmed 
                && existingUser.RegistrationDate.AddHours(EmailConfirmationLifeTime) < DateTime.Now)
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

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid Request");
            if (!user.EmailConfirmed)
            {
                string errorMessage;
                if (user.RegistrationDate.AddHours(EmailConfirmationLifeTime) < DateTime.Now)
                {
                    await _userManager.DeleteAsync(user);
                    errorMessage = "Invalid request";
                }
                else
                    errorMessage = "Confirm your email first";
                return BadRequest(errorMessage);
            };
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var param = new Dictionary<string, string?>
            {
                {"token", token },
                {"email", model.Email }
            };
            var callback = QueryHelpers.AddQueryString(model.ClientURI, param);

            string data = System.IO.File.ReadAllText(Path.Combine(_fileManager.GetSolutionPath()!, "WebAPI\\Mails\\ResetPassword.html"));
            var doc = new HtmlDocument();
            doc.LoadHtml(data);
            var logo = doc.GetElementbyId("logo");
            logo.Attributes.Add("src", $"data:image/png;base64, {Convert.ToBase64String(await _fileManager.GetFileContentAsync("logo.png", Business.Enums.EasyWorkFileTypes.EasyWorkProjectImage))}");
            var link = doc.GetElementbyId("link");
            link.Attributes.Add("href", callback);
            await _mailService.SendAsync(new Business.Other.MailRequest()
            {
                To = user.Email,
                Subject = "EasyWork: Password reset request",
                Body = doc.DocumentNode.InnerHtml
            });
            return Ok();
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user is null)
                return BadRequest("Invalid Request");
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            if (!resetPasswordResult.Succeeded)
            {
                var errors = resetPasswordResult.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }
            return Ok();
        }

        // NOT TESTED!
        [HttpPost("ExternalLogin")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthDTO externalAuth)
        {
            var payload = await _tokenService.VerifyGoogleTokenAsync(externalAuth.IdToken);
            if (payload is null)
                return BadRequest("Invalid External Authentication.");
            var info = new UserLoginInfo(externalAuth.Provider, payload.Subject, externalAuth.Provider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
                if (user is null)
                {
                    user = new User 
                    { 
                        Email = payload.Email, 
                        UserName = payload.Email,
                        FirstName = payload.GivenName,
                        LastName = payload.FamilyName,
                        RegistrationDate = DateTime.Now,
                        EmailConfirmed = true
                    };
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "User");
                    if (payload.Picture is not null)
                    {
                        var client = new HttpClient();
                        var picture = await client.GetAsync(payload.Picture);
                        var type = picture.Content.Headers.ContentType?.MediaType;
                        if (type is not null)
                        {
                            var notMIME = _fileManager.GetImageType(type);
                            if (notMIME is not null)
                            {
                                await _userAvatarService.UpdateAvatarAsync(user.Id, await picture.Content.ReadAsByteArrayAsync(), type);
                            }
                        }
                    }
                    await _userManager.AddLoginAsync(user, info);
                }
                else
                {
                    if (!user.EmailConfirmed)
                    {
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);
                    }
                    await _userManager.AddLoginAsync(user, info);
                }
            }
            if (user is null)
                return BadRequest("Invalid External Authentication.");

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
            var accessToken = _tokenService.GenerateAccessToken(await GetClaimsAsync(user));
            return Ok(new LoginResponseDTO {
                Token = new TokenDTO()
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }, IsAuthSuccessful = true });
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(UpdateUserDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name);
            user = _mapper.Map(model, user);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return NoContent();
        }

        [Authorize]
        [Route("LastSeen")]
        [HttpPut]
        public async Task<IActionResult> UpdateLastSeen()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name);
            user.LastSeen = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return NoContent();
        }

        [Authorize]
        [Route("Avatar")]
        [HttpPut]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            try
            {
                await _userAvatarService.UpdateAvatarAsync(userId.Value, file);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(new
                {
                    errors = new
                    {
                        exc.Message
                    }
                });
            }
            return NoContent();
        }

        [Authorize]
        [Route("Avatar")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAvatar()
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            try
            {
                await _userAvatarService.DeleteAvatarByUserIdAsync(userId.Value);
            }
            catch (InvalidOperationException exc)
            {
                return NotFound(new
                {
                    errors = new
                    {
                        exc.Message
                    }
                });
            }
            return NoContent();
        }
    }
}
