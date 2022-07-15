import { TaskPriority } from "./priority/task-priority";
import { TaskStatus } from "./status/task-status";

export interface TaskModel {
    id: number;

    name: string;

    description: string | null | undefined;

    startDate: string;

    deadline: string | null | undefined;

    endDate: string | null | undefined;

    status: TaskStatus;

    priority: TaskPriority | null | undefined;

    projectId: number;
}
