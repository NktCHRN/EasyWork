import { UserMiniWithAvatarModel } from "../user/user-mini-with-avatar.model";

export interface MessageModel {
    id: number;
    text: string;
    date: string;
    sender: UserMiniWithAvatarModel;
}
