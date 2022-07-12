import { UserMiniWithAvatarModel } from "../../user/user-mini-with-avatar.model";
import { UserOnProjectRole } from "../role/user-on-project-role";

export interface UserOnProjectExtendedModel {
    user: UserMiniWithAvatarModel;
    role: UserOnProjectRole;
    tasksDone: number;
    tasksNotDone: number;
    isKickable: boolean;
}
