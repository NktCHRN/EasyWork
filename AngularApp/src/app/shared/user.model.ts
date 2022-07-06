export interface UserModel
{
    email: string;
    phoneNumber: string | null | undefined;
    firstName: string;
    lastName: string | null | undefined;
    avatarURL: string | null | undefined;
    tasksDone: number;
    tasksNotDone: number;
    projects: number;
}
