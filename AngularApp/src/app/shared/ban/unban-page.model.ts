import { ConnectionContainer } from "../other/connection-container";
import { UserProfileReducedModel } from "../user/user-profile-reduced.model";

export interface UnbanPageModel
{
    user: UserProfileReducedModel;
    connectionContainer: ConnectionContainer;
}
