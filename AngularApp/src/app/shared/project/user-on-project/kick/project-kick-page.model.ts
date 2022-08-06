import { ConnectionContainer } from "src/app/shared/other/connection-container";
import { UserMiniReducedModel } from "../../../user/user-mini-reduced.model";
import { ProjectMiniModel } from "../../project-mini.model";

export interface ProjectKickPageModel {
    project: ProjectMiniModel;
    toKick: UserMiniReducedModel;
    connectionContainer: ConnectionContainer;
}
