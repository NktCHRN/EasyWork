import { TaskStatus } from "./status/task-status";
import { TaskReducedModel } from "./task-reduced.model";

export interface TaskReducedWithStatusModel {
    task: TaskReducedModel;
    status: TaskStatus;
}
