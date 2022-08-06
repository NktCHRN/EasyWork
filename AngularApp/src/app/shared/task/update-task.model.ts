import { TaskPriority } from "./priority/task-priority";
import { TaskStatus } from "./status/task-status";

export interface UpdateTaskModel {
    name: string;

    description: string | null | undefined;

    deadline: string | null | undefined;

    endDate: string | null | undefined;

    status: TaskStatus;

    priority: TaskPriority | null | undefined;
}
