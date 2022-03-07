using Business.Interfaces;
using Business.Models;
using Business.Services;
using Data;
using Data.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Tests.BLLTests
{
    [TestFixture]
    public class BanServiceTests
    {
        private ApplicationDbContext _context = null!;

        private IBanService _service = null!;

        private readonly IEnumerable<BanModel> _invalidBans = new BanModel[]
        {
            new BanModel()  // 0
            {
                AdminId = 1,
                UserId = 2,
                Hammer = "Lorem ipsum",
                DateTo = DateTime.Now.AddMonths(-1)
            },
            new BanModel()  // 1
                {
                    AdminId = 1,
                    UserId = 4,
                    Hammer = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                    "Suspendisse porttitor fermentum porta. Proin sagittis, lacus id sollicitudin vehicula, " +
                    "velit augue sagittis lacus, eget lobortis ipsum ipsum in tortor. Quisque nunc nulla, " +
                    "pulvinar ut aliquam sed, vestibulum nec nisl. Nunc nec lacus id sapien posuere malesuada. " +
                    "Donec a lorem non tortor sollicitudin lobortis. Morbi et consectetur lacus augue. " +
                "More than 400 bytes (characters)!",
                    DateTo =  DateTime.Now.AddMonths(1)
                },
            new BanModel()  // 2
            {
                AdminId = 1,
                Hammer = "Lorem ipsum",
                DateTo = DateTime.Now.AddMonths(1)
            },
            new BanModel()  // 3
            {
                AdminId = -1,
                UserId = 2,
                Hammer = "Lorem ipsum",
                DateTo = DateTime.Now.AddMonths(1)
            }
        };

        private readonly BanModel _validBan = new BanModel()
        {
            AdminId = 1,
            UserId = 2,
            Hammer = "Lorem ipsum",
            DateTo = DateTime.Now.AddMonths(1)
        };

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            _service = new BanService(_context, UnitTestHelper.CreateMapperProfile());
        }

        private void SeedData()
        {
            var bans = new Ban[]
            {
                new Ban()   // id 1
                {
                    AdminId = 1,
                    UserId = 2,
                    Hammer = "Lorem ipsum",
                    DateFrom = new DateTime(2021, 12, 1),
                    DateTo = new DateTime(2022, 12, 1)
                },
                new Ban()   // id 2
                {
                    AdminId = 1,
                    UserId = 3,
                    DateFrom = new DateTime(2022, 1, 10),
                    DateTo = new DateTime(2022, 2, 25)
                },
                new Ban()   // id 3
                {
                    AdminId = 1,
                    UserId = 4,
                    Hammer = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                    "Suspendisse porttitor fermentum porta. Proin sagittis, lacus id sollicitudin vehicula, " +
                    "velit augue sagittis lacus, eget lobortis ipsum ipsum in tortor. Quisque nunc nulla, " +
                    "pulvinar ut aliquam sed, vestibulum nec nisl. Nunc nec lacus id sapien posuere malesuada. " +
                    "Donec a lorem non tortor sollicitudin lobortis. Morbi et consectetur lacus augue.",
                    DateFrom = new DateTime(2022, 3, 8, 1, 0, 0),
                    DateTo = new DateTime(2022, 3, 8, 3, 0, 0)
                }
            };
            foreach (var ban in bans)
            {
                _context.Bans.Add(ban);
                _context.SaveChanges();
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException(int modelNumber)
        {
            // Arrange
            var model = _invalidBans.ElementAt(modelNumber);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model),
                "Method does not throw an ArgumentException if model is invalid");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsModel()
        {
            // Arrange
            SeedData();
            var expectedCount = _context.Bans.Count() + 1;

            // Act
            await _service.AddAsync(_validBan);
            var actualCount = _context.Bans.Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount, "Method does not adds an element");
        }


        // TODO:
        // Add tests for other methods
        // Add documentation for all methods
    }
}
