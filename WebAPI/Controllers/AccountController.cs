using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Other;
using Business.Models;

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

        private readonly IUserStatsService _userStatsService;

        private readonly IRefreshTokenService _refreshTokenService;

        private int EmailConfirmationLifeTime => 
            int.Parse(_configuration.GetSection("TokenProvidersSetting:EmailConfirmationLifetime").Value);

        private int RefreshTokenLifeTime =>
            int.Parse(_configuration.GetSection("TokenProvidersSetting:RefreshTokenLifetimeInDays").Value);

        public AccountController(UserManager<User> userManager, ITokenService tokenService, IMapper mapper, IBanService banService, IFileManager fileManager, IMailService mailService, IConfiguration configuration, IUserAvatarService userAvatarService, IUserStatsService userStatsService, IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _banService = banService;
            _fileManager = fileManager;
            _mailService = mailService;
            _configuration = configuration;
            _userAvatarService = userAvatarService;
            _userStatsService = userStatsService;
            _refreshTokenService = refreshTokenService;
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
            {
                return Unauthorized(new LoginResponseDTO()
                {
                    ErrorMessage = "You are banned from this website",
                    ErrorDetails = await MapBansAsync(bans)
                });
            }
            user.LastSeen = DateTime.UtcNow;
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _refreshTokenService.AddAsync(new RefreshTokenModel
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenLifeTime)
        });
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

        internal async Task<IEnumerable<BannedUserDTO>> MapBansAsync(IEnumerable<BanModel> bans)
        {
            var mappedBans = new List<BannedUserDTO>();
            foreach (var ban in bans)
            {
                var mappedBan = _mapper.Map<BannedUserDTO>(ban);
                var admin = await _userManager.FindByIdAsync(ban.AdminId.GetValueOrDefault().ToString());
                if (admin is not null)
                {
                    var adminModel = new UserMiniDTO()
                    {
                        Id = admin.Id,
                        Email = admin.Email,
                        FullName = $"{admin.FirstName} {admin.LastName}".TrimEnd(),
                    };
                    mappedBan = mappedBan with
                    {
                        Admin = adminModel
                    };
                }
                mappedBans.Add(mappedBan);
            }
            return mappedBans;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = _mapper.Map<User>(model);
            user.RegistrationDate = DateTime.UtcNow;
            user.UserName = model.Email;
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser is not null 
                && !existingUser.EmailConfirmed 
                && existingUser.RegistrationDate.AddHours(EmailConfirmationLifeTime) < DateTime.UtcNow)
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

        [HttpPost("EmailConfirmationMessageResend")]
        public async Task<IActionResult> ResendConfirmationMessage([FromBody] ResendEmailConfirmationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _userManager.FindByNameAsync(dto.Email);
            if (user is null)
                return NotFound();
            if (user.EmailConfirmed)
                return BadRequest("You have already confirmed your email");
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
            var callback = QueryHelpers.AddQueryString(dto.ClientURI, param);
            link.Attributes.Add("href", callback);
            await _mailService.SendAsync(new Business.Other.MailRequest()
            {
                To = dto.Email,
                Subject = "EasyWork: Confirm your email",
                Body = doc.DocumentNode.InnerHtml
            });
            return NoContent();
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
                if (user.RegistrationDate.AddHours(EmailConfirmationLifeTime) < DateTime.UtcNow)
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
                return BadRequest(errors);
            }
            await _refreshTokenService.DeleteUserTokensAsync(user.Id);
            return Ok();
        }

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
                        RegistrationDate = DateTime.UtcNow,
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
                                await _userAvatarService
                                    .UpdateAvatarAsync(user.Id, await picture.Content.ReadAsByteArrayAsync(), notMIME);
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
            {
                return Unauthorized(new LoginResponseDTO()
                {
                    ErrorMessage = "You are banned from this website",
                    ErrorDetails = await MapBansAsync(bans)
                });
            }

            user.LastSeen = DateTime.UtcNow;
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _refreshTokenService.AddAsync(new RefreshTokenModel
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenLifeTime)
            });
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
            user.LastSeen = DateTime.UtcNow;
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
                return BadRequest(exc.Message);
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
                return NotFound(exc.Message);
            }
            return NoContent();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name);
            var stats = _userStatsService.GetStatsById(user.Id);
            string? avatarType = null;
            string? avatarURL = null;
            if (user.AvatarFormat is not null)
            {
                avatarType = _fileManager.GetImageMIMEType(user.AvatarFormat);
                avatarURL = $"{this.GetApiUrl()}Users/{user.Id}/Avatar";
            }
            var model = _mapper.Map<UserCabinetProfileDTO>(user);
            return Ok(model with
            {
                MIMEAvatarType = avatarType,
                AvatarURL = avatarURL,
                Projects = stats.Projects,
                TasksDone = stats.TasksDone,
                TasksNotDone = stats.TasksNotDone
            });
        }
    }
}
