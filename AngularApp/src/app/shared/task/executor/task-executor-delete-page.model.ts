import { ConnectionContainer } from "../../other/connection-container";
import { UserMiniReducedModel } from "../../user/user-mini-reduced.model";

export interface TaskExecutorDeletePageModel {
    user: UserMiniReducedModel;
    taskId: number;
    connectionContainer: ConnectionContainer;
}
