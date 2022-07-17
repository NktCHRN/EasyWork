import { TaskStatus } from "./status/task-status";
import { TaskReducedModel } from "./task-reduced.model";

export interface TaskReducedWithStatusModel extends TaskReducedModel {
    status: TaskStatus;
}
