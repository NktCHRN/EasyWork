import { ProjectMiniModel } from "./project-mini.model";
import { UserOnProjectExtendedModel } from "./user-on-project/user-on-project-extended.model";

export interface ProjectMiniWithUsersModel extends ProjectMiniModel {
    users: UserOnProjectExtendedModel[]
}
