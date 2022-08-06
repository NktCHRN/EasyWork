import { ConnectionContainer } from "../other/connection-container";
import { UserProfileReducedModel } from "../user/user-profile-reduced.model";

export interface BanAddPageModel
{
    user: UserProfileReducedModel;
    connectionContainer: ConnectionContainer;
}
