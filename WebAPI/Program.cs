using Business;
using Business.Identity;
using Business.Interfaces;
using Business.Managers;
using Business.Other;
using Business.Services;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;
using System.Text;
using WebAPI;
using WebAPI.Authorization;
using WebAPI.Data;
using WebAPI.TokenProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddIdentityCore<User>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
    options.Tokens.PasswordResetTokenProvider = "CustomPasswordReset";
})
    .AddUserValidator<CustomPropertiesUserValidator>()
    .AddUserValidator<PhoneNumberUserValidator>()
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<CustomEmailConfirmationTokenProvider<User>>("CustomEmailConfirmation")
    .AddTokenProvider<CustomPasswordResetTokenProvider<User>>("CustomPasswordReset");

builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetSection("JwtBearer:Issuer").Value,

            ValidateAudience = true,
            ValidAudience = builder.Configuration.GetSection("JwtBearer:Audience").Value,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtBearer:Secret").Value)),
            ValidateIssuerSigningKey = true,
            
            ValidateLifetime = true
        };
    });
builder.Services.AddTransient<IAuthorizationHandler, NotBannedHandler>();
builder.Services.AddAuthorization(options => {
    var userAuthPolicyBuilder = new AuthorizationPolicyBuilder();
    options.DefaultPolicy = userAuthPolicyBuilder
                                    .RequireAuthenticatedUser()
                                    .RequireClaim(ClaimTypes.NameIdentifier)
                                    .AddRequirements(new NotBannedRequirement())
                                    .Build();
});

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, NotBannedAuthorizationMiddlewareResultHandler>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

builder.Services.AddScoped<IFileManager, FileManager>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBanService, BanService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserAvatarService, UserAvatarService>();
builder.Services.AddScoped<IUserOnProjectService, UserOnProjectService>();
builder.Services.AddScoped<IUserStatsService, UserStatsService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddAutoMapper(typeof(AutoMapperBusinessProfile), typeof(AutoMapperWebAPIProfile));
builder.Services.AddCors(setup =>
{
    setup.AddPolicy("default", options =>
    {
        options.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
});

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<CustomEmailConfirmationTokenProviderOptions>(opt =>
        opt.TokenLifespan = TimeSpan
        .FromHours(int.Parse(builder.Configuration.GetSection("TokenProvidersSetting:EmailConfirmationLifetime").Value)));
builder.Services.Configure<CustomPasswordResetTokenProviderOptions>(opt =>
        opt.TokenLifespan = TimeSpan
        .FromHours(int.Parse(builder.Configuration.GetSection("TokenProvidersSetting:PasswordResetLifetime").Value)));
const long maxFileSize = 10737418240;                       // 10 GB
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = maxFileSize;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = maxFileSize;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxFileSize;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("default");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using var serviceScope = app.Services.CreateScope();
var seeder = new DataSeeder()
{
    RoleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole<int>>>()!,
    UserManager = serviceScope.ServiceProvider.GetService<UserManager<User>>()!
};
await seeder.SeedRoles();
await seeder.SeedUsers();

app.Run();
