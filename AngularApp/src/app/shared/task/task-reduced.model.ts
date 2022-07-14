export interface TaskReducedModel {
    id: number

    name: string

    startDate: string

    deadline: string | null | undefined

    endDate: string | null | undefined

    messagesCount: number;

    filesCount: number;
}
