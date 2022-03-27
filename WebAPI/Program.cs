using Business;
using Business.Identity;
using Business.Interfaces;
using Business.Managers;
using Business.Services;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddUserValidator<CustomPropertiesUserValidator>()
    .AddUserValidator<PhoneNumberUserValidator>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAutoMapper(typeof(AutoMapperBusinessProfile));
builder.Services.AddScoped<IFileManager, FileManager>();
builder.Services.AddScoped<IBanService, BanService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IReleaseService, ReleaseService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserAvatarService, UserAvatarService>();
builder.Services.AddScoped<IUserOnProjectService, UserOnProjectService>();
builder.Services.AddScoped<IUserStatsService, UserStatsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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