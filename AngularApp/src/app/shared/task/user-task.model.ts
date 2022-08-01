import { TaskExtraReducedModel } from "./task-extra-reduced.model";

export interface UserTaskModel extends TaskExtraReducedModel {
    startDate: string;

    deadline: string | null | undefined;

    endDate: string | null | undefined;

    status: string;

    projectId: number;
}
