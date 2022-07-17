export interface UserTaskModel {
    id: number;

    name: string;

    startDate: string;

    deadline: string | null | undefined;

    endDate: string | null | undefined;

    status: string;
}
