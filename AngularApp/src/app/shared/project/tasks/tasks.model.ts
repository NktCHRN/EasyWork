import { TaskReducedModel } from "../../task/task-reduced.model";

export interface TasksModel {
    toDo: TaskReducedModel[];
    inProgress: TaskReducedModel[];
    validate: TaskReducedModel[];
    done: TaskReducedModel[];
}
