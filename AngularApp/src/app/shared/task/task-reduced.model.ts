import { TaskPriority } from "./priority/task-priority";

export interface TaskReducedModel {
    id: number;

    name: string;

    startDate: string;

    deadline: string | null | undefined

    endDate: string | null | undefined;

    priority: TaskPriority | null | undefined;

    messagesCount: number;

    filesCount: number;
}
