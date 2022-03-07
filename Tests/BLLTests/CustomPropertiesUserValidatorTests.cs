using Business.Identity;
using Data.Entities;
using NUnit.Framework;

namespace Tests.BLLTests
{
    [TestFixture]
    public class CustomPropertiesUserValidatorTests
    {
        private CustomPropertiesUserValidator _validator = null!;

        [SetUp]
        public void Setup()
        {
            _validator = new CustomPropertiesUserValidator();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur vestibulum.")]
        public void ValidateCustomProperties_InvalidFirstName_ReturnsFailed(string firstName)
        {
            // Arrange
            var user = new User()
            {
                FirstName = firstName,
                LastName = "Valid"
            };

            // Act
            var actual = _validator.ValidateAsync(null!, user).Result;

            // Assert
            Assert.IsFalse(actual.Succeeded, "Method does not set false to succeeded parameter if first name is invalid");
        }

        [Test]
        [TestCase("Nick")]
        [TestCase("N")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur vestibulum")]
        public void ValidateCustomProperties_ValidFirstName_ReturnsSuccess(string firstName)
        {
            // Arrange
            var user = new User()
            {
                FirstName = firstName,
                LastName = "Valid"
            };

            // Act
            var actual = _validator.ValidateAsync(null!, user).Result;

            // Assert
            Assert.IsTrue(actual.Succeeded, "Method does not set true to succeeded parameter if first name is valid");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur vestibulum.")]
        public void ValidateCustomProperties_InvalidLastName_ReturnsFailed(string lastName)
        {
            // Arrange
            var user = new User()
            {
                FirstName = "Valid",
                LastName = lastName
            };

            // Act
            var actual = _validator.ValidateAsync(null!, user).Result;

            // Assert
            Assert.IsFalse(actual.Succeeded, "Method does not set false to succeeded parameter if last name is invalid");
        }

        [Test]
        [TestCase("Hornet")]
        [TestCase("h")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur vestibulum")]
        public void ValidateCustomProperties_ValidLastName_ReturnsSuccess(string lastName)
        {
            // Arrange
            var user = new User()
            {
                FirstName = "Valid",
                LastName = lastName
            };

            // Act
            var actual = _validator.ValidateAsync(null!, user).Result;

            // Assert
            Assert.IsTrue(actual.Succeeded, "Method does not set true to succeeded parameter if last name is valid");
        }

        [Test]
        [TestCase("")]
        [TestCase("bm")]
        [TestCase(".jpeg")]
        public void ValidateCustomProperties_InvalidAvatarFormat_ReturnsFailed(string format)
        {
            // Arrange
            var user = new User()
            {
                FirstName = "Valid",
                LastName = "Valid",
                AvatarFormat = format
            };

            // Act
            var actual = _validator.ValidateAsync(null!, user).Result;

            // Assert
            Assert.IsFalse(actual.Succeeded, "Method does not set false to succeeded parameter if the avatar format is invalid");
        }

        [Test]
        [TestCase(null)]
        [TestCase("bmp")]
        [TestCase("png")]
        [TestCase("jpg")]
        [TestCase("jpeg")]
        public void ValidateCustomProperties_ValidAvatarFormat_ReturnsSuccess(string format)
        {
            // Arrange
            var user = new User()
            {
                FirstName = "Valid",
                LastName = "Valid",
                AvatarFormat = format
            };

            // Act
            var actual = _validator.ValidateAsync(null!, user).Result;

            // Assert
            Assert.IsTrue(actual.Succeeded, "Method does not set true to succeeded parameter if the avatar format is valid");
        }
    }
}
