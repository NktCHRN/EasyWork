import { TaskStatus } from "./task-status";

export interface TaskStatusWithDescription {
    status: TaskStatus;
    description: string;
}
