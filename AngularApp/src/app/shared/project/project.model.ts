export interface ProjectModel {
    id: number;
    name: string;
    description: string | null | undefined;
    startDate: string;
    maxToDo: number | null | undefined;
    maxInProgress: number | null | undefined;
    maxValidate: number | null | undefined;
    inviteCode: string | null | undefined;
    isInviteCodeActive: boolean;
}
