﻿using Business.Interfaces;
using Business.Other;
using Business.Services;
using Data;
using Data.Entities;
using NUnit.Framework;
using TaskEntity = Data.Entities.Task;

namespace Tests.BLLTests
{
    [TestFixture]
    public class UserStatsServiceTests
    {
        private ApplicationDbContext _context = null!;

        private IUserStatsService _service = null!;

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedData();
            _service = new UserStatsService(_context);
        }

        public void SeedData()
        {
            var projects = new Project[]
            {
                new Project()       // id 1
                {
                    Name = "Project 1",
                    OwnerId = 1
                },
                new Project()       // id 2
                {
                    Name = "Project 2",
                    OwnerId = 3
                },
                new Project()       // id 3
                {
                    Name = "Project 3",
                    OwnerId = 5
                },
            };
            foreach (var project in projects)
            {
                _context.Projects.Add(project);
                _context.SaveChanges();
            }

            var uops = new UserOnProject[]
            {
                new UserOnProject()
                {
                    ProjectId = 2,
                    UserId = 1,
                    IsManager = false
                }
            };
            foreach (var uop in uops)
            {
                _context.UsersOnProjects.Add(uop);
                _context.SaveChanges();
            }

            var tasks = new TaskEntity[]
            {
                new TaskEntity()
                {
                    Name = "Task 1",
                    ProjectId = 1,
                    ExecutorId = 1,
                    Status = TaskStatuses.Archived
                },
                new TaskEntity()
                {
                    Name = "Task 2",
                    ProjectId = 1,
                    ExecutorId = 1,
                    Status = TaskStatuses.Validate
                },
                new TaskEntity()
                {
                    Name = "Task 3",
                    ProjectId = 2,
                    ExecutorId = 1,
                    Status = TaskStatuses.Complete
                },
                new TaskEntity()
                {
                    Name = "Task 4",
                    ProjectId = 1,
                    ExecutorId = 1,
                    Status = TaskStatuses.ToDo
                },
                new TaskEntity()
                {
                    Name = "Task 5",
                    ProjectId = 2,
                    ExecutorId = 1,
                    Status = TaskStatuses.InProgress
                }
            };
            foreach (var task in tasks)
            {
                _context.Tasks.Add(task);
                _context.SaveChanges();
            }
        }

        [Test]
        public void GetStatsByIdTest_ReturnRightStats()
        {
            // Arrange
            var userId = 1;
            var expected = new UserStats()
            {
                UserId = userId,
                Projects = 2,
                TasksDone = 3,
                TasksNotDone = 2
            };

            // Act
            var actual = _service.GetStatsById(userId);

            // Assert
            Assert.AreEqual(expected, actual, "Method returnes wrong user statistics");
        }
    }
}
