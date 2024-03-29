export class UserCabinetModel
{
    email: string = '';
    phoneNumber: string | null | undefined;
    firstName: string = '';
    lastName: string | null | undefined;
    registrationDate: string = '';
    avatarURL: string | null | undefined;
    tasksDone: number = 0;
    tasksNotDone: number = 0;
    projects: number = 0;
}
