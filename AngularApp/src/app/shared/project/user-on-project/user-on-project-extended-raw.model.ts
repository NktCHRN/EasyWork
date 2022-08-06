import { UserMiniWithAvatarModel } from "../../user/user-mini-with-avatar.model";

export interface UserOnProjectExtendedRawModel {
    user: UserMiniWithAvatarModel;
    role: string;
    tasksDone: number;
    tasksNotDone: number;
}
