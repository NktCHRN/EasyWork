using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Task = System.Threading.Tasks.Task;

namespace WebAPI.Data
{
    public class DataSeeder
    {
        public RoleManager<IdentityRole<int>> RoleManager { get; set; } = null!;

        public UserManager<User> UserManager { get; set; } = null!;

        public async Task SeedRoles()
        {
            var roles = new string[]
            {
                "User",
                "Admin"
            };
            foreach (var role in roles)
            {
                if (await RoleManager.FindByNameAsync(role) is null)
                    await RoleManager.CreateAsync(new IdentityRole<int> { Name = role, NormalizedName = role.ToUpperInvariant() });
            }
        }

        public async Task SeedUsers()
        {
            var user = new User()
            {
                Email = "14nik20@gmail.com",
                FirstName = "Nikita",
                LastName = "Chernikov",
                RegistrationDate = DateTime.Now
            };
            // you may also add a phone number
            user.NormalizedEmail = user.Email.ToUpperInvariant();
            user.UserName = user.Email;
            user.NormalizedUserName = user.NormalizedEmail;
            var password = "P4ssw0rd";                          // initial password. Should be changed after the creation
            if (await UserManager.FindByEmailAsync(user.Email) is null)
                await UserManager.CreateAsync(user, password);
        }
    }
}
