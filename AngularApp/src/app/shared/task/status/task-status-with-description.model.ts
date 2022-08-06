import { TaskStatus } from "./task-status";

export interface TaskStatusWithDescriptionModel {
    status: TaskStatus;
    description: string;
}
