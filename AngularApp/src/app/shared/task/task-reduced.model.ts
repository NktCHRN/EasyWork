import { UserMiniWithAvatarModel } from "../user/user-mini-with-avatar.model";
import { TaskPriority } from "./priority/task-priority";
import { TaskExtraReducedModel } from "./task-extra-reduced.model";

export interface TaskReducedModel extends TaskExtraReducedModel {
    startDate: string;

    deadline: string | null | undefined

    endDate: string | null | undefined;

    priority: TaskPriority | null | undefined;

    messagesCount: number;

    filesCount: number;

    executors: UserMiniWithAvatarModel[] | null | undefined;
}
