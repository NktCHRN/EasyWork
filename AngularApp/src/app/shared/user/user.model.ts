export class UserModel
{
    email: string = '';
    phoneNumber: string | null | undefined;
    firstName: string = '';
    lastName: string | null | undefined;
    lastSeen: string | null | undefined;
    isOnline: boolean = false;
    avatarURL: string | null | undefined;
    tasksDone: number = 0;
    tasksNotDone: number = 0;
    projects: number = 0;
}
