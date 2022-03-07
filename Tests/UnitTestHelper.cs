using AutoMapper;
using Business;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Tests
{
    internal class UnitTestHelper
    {
        public static DbContextOptions<ApplicationDbContext> GetUnitTestDbOptions()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                SeedData(context);
            }
            return options;
        }

        public static void SeedData(ApplicationDbContext context)
        {
            var users = new User[]
            {
                new User()  // id 1
                {
                    FirstName = "Michael",
                    LastName = "Gordon",
                    PhoneNumber = "380677123238",
                    Email = "mg23112@fakemail.com"
                },
                new User()  // id 2
                {
                    FirstName = "Nick",
                    LastName = "Flame",
                    PhoneNumber = "380777333298",
                    Email = "777@nomail.com"
                },
                new User()  // id 3
                {
                    FirstName = "Rick",
                    LastName = "Michaelson",
                    PhoneNumber = "380991233238",
                    Email = "rm@notamail.com",
                },
                new User()  // id 4
                {
                    FirstName = "Martin",
                    LastName = "Wright",
                    Email = "mwright@mail.com"
                },
                new User()  // id 5
                {
                    FirstName = "Peter",
                    LastName = "Radd",
                    Email = "prd@pmail.net"
                }
            };
            foreach (var user in users)
            {
                user.NormalizedEmail = user.Email.ToUpperInvariant();
                user.UserName = user.Email;
                user.NormalizedUserName = user.NormalizedEmail;
                context.Users.Add(user);
                context.SaveChanges();
            }
        }

        public static Mapper CreateMapperProfile()
        {
            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            return new Mapper(configuration);
        }
    }
}
