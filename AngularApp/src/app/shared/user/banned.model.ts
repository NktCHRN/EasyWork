import { UserMiniModel } from "./user-mini.model"

export interface BannedModel {
    dateFrom: string
    dateTo: string
    hammer: string | undefined | null
    admin: UserMiniModel
}
