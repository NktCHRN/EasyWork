import { ConnectionContainer } from "src/app/shared/other/connection-container";
import { ProjectMiniModel } from "../../project-mini.model";
import { UserOnProjectRole } from "../role/user-on-project-role";
import { UserOnProjectExtendedModel } from "../user-on-project-extended.model";

export interface ProjectEditUserPageModel {
    project: ProjectMiniModel;
    user: UserOnProjectExtendedModel;
    myRole: UserOnProjectRole;
    connectionContainer: ConnectionContainer;
}
