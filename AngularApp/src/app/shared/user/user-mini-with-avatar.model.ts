import { UserMiniReducedModel } from "./user-mini-reduced.model";

export interface UserMiniWithAvatarModel extends UserMiniReducedModel {
    avatarURL: string | null | undefined;
}
