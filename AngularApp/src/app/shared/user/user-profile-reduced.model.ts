export interface UserProfileReducedModel
{
    id: number;
    fullName: string;
    email: string;
    avatarURL: string | null | undefined;
    lastSeen: string | null | undefined;
}
