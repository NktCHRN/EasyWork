import { ConnectionContainer } from "src/app/shared/other/connection-container";
import { ProjectMiniWithUsersModel } from "../../project-mini-with-users.model";
import { UserOnProjectRole } from "../role/user-on-project-role";

export interface ProjectAddUserPageModel {
    project: ProjectMiniWithUsersModel;
    myRole: UserOnProjectRole;
    connectionContainer: ConnectionContainer;
}
