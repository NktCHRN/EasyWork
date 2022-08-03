import { GanttTaskModel } from "./gantt-task.model";

export interface GanttModel
{
    startDate: string;
    endDate: string;
    tasks: GanttTaskModel[]
}
