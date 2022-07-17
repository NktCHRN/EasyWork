import { TaskStatus } from "./task-status";

export interface TaskStatusChangeModel {
    id: number,
    old: TaskStatus;
    new: TaskStatus;
}
