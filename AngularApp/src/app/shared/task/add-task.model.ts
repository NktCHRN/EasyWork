import { TaskStatus } from "./status/task-status";

export interface AddTaskModel {
    name: string;
    status: TaskStatus;
}
