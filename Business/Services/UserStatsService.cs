﻿using Business.Interfaces;
using Business.Other;
using Data;

namespace Business.Services
{
    public class UserStatsService : IUserStatsService
    {
        private readonly ApplicationDbContext _context;

        public UserStatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public UserStats GetStatsById(int userId)
        {
            var userTasks = _context.Tasks.Where(t => t.ExecutorId == userId);
            var tasksDoneCount = userTasks.AsEnumerable().Where(t => TaskService.IsDone(t.Status)).Count();
            return new UserStats()
            {
                UserId = userId,
                Projects = _context.Projects
                .Where(p => _context.UsersOnProjects
                    .Any(uop => uop.ProjectId == p.Id && uop.UserId == userId))
                .Count(),
                TasksDone = tasksDoneCount,
                TasksNotDone = userTasks.Count() - tasksDoneCount
            };
        }
    }
}
