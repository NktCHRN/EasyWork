import { ProjectMiniWithUsersModel } from "../../project-mini-with-users.model";
import { UserOnProjectRole } from "../role/user-on-project-role";

export interface ProjectAddUserPageModel {
    project: ProjectMiniWithUsersModel;
    myRole: UserOnProjectRole;
}
