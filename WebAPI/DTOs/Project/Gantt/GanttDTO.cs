﻿namespace WebAPI.DTOs.Project.Gantt
{
    public record GanttDTO
    {
        public DateTimeOffset StartDate { get; init; }

        public DateTimeOffset EndDate { get; init; }

        public IEnumerable<GanttTaskDTO> Tasks { get; init; } = new List<GanttTaskDTO>();
    }
}
