using Business.Identity;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests.BLLTests
{
    [TestFixture]
    public class PhoneNumberUserValidatorTests
    {
        private UserManager<User> _manager = null!;

        private PhoneNumberUserValidator _validator = null!;

        [SetUp]
        public void Setup()
        {
            var store = new UserStore<User, IdentityRole<int>, ApplicationDbContext, int>(new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions()));
            _manager = new UserManager<User>(store, null!, null!, null!, null!, null!, null!, null!, null!);
            _validator = new PhoneNumberUserValidator();
        }

        [Test]
        [TestCase("+14167129018")]
        [TestCase("+38(068)2337711")]
        [TestCase("1-234-567-8901 ext1234")]
        [TestCase("1 (234) 567-8901")]
        [TestCase("1.234.567.8901")]
        [TestCase("1/234/567/8901")]
        [TestCase("380677123238")]
        [TestCase("")]
        public void ValidatePhoneNumber_InvalidPhoneNumber_ReturnFailed(string phoneNumber)
        {
            // Arrange
            var user = new User()
            {
                FirstName = "Valid",
                LastName = "Valid",
                PhoneNumber = phoneNumber
            };

            // Act
            var actual = _validator.ValidateAsync(_manager, user).Result;

            // Assert
            Assert.IsFalse(actual.Succeeded, "Method does not set false to succeeded parameter if the phone number is invalid");
        }

        [Test]
        [TestCase("12345678901")]
        [TestCase("123456789011234")]
        [TestCase("380689789456")]
        [TestCase("67712345")]
        [TestCase(null)]
        public void ValidatePhoneNumber_ValidPhoneNumber_ReturnSuccess(string phoneNumber)
        {
            // Arrange
            var user = new User()
            {
                FirstName = "Valid",
                LastName = "Valid",
                PhoneNumber = phoneNumber
            };

            // Act
            var actual = _validator.ValidateAsync(_manager, user).Result;

            // Assert
            Assert.IsTrue(actual.Succeeded, "Method does not set true to succeeded parameter if the phone number is valid");
        }
    }
}
