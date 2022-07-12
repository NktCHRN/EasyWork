import { UserOnProjectRole } from "../role/user-on-project-role";

export class UserOnProjectReducedModel {
    userId: number = 0;
    role: UserOnProjectRole = UserOnProjectRole.User;
}
