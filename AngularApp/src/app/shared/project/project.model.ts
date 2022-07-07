export interface ProjectModel {
    id: number;
    name: string;
    description: string | null | undefined;
    startDate: string;
    inviteCode: string | null | undefined;
    isInviteCodeActive: boolean;
}
