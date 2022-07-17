import { TaskPriority } from "./priority/task-priority";
import { TaskStatus } from "./status/task-status";
import { TaskExtraReducedModel } from "./task-extra-reduced.model";

export interface TaskModel extends TaskExtraReducedModel {
    description: string | null | undefined;

    startDate: string;

    deadline: string | null | undefined;

    endDate: string | null | undefined;

    status: TaskStatus;

    priority: TaskPriority | null | undefined;

    projectId: number;
}
