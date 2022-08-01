import { ConnectionContainer } from "../../other/connection-container";
import { TaskExtraReducedModel } from "../task-extra-reduced.model";

export interface TaskDeleteDialogSettingsModel {
    task: TaskExtraReducedModel,
    projectsConnection: ConnectionContainer
}
