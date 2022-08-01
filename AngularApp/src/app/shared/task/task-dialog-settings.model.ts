import { ConnectionContainer } from "../other/connection-container";
import { ProjectLimitsModel } from "../project/limits/project-limits.model";
import { TasksCountModel } from "../project/tasks/tasks-count.model";

export interface TaskDialogSettingsModel {
    taskId: number;
    showToProjectButton: boolean;
    limits: ProjectLimitsModel;
    tasksCount: TasksCountModel;
    projectsConnectionContainer: ConnectionContainer;
}
