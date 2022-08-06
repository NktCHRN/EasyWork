import { ConnectionContainer } from "../other/connection-container";

export interface BanDeletePageModel
{
    id: number;
    userId: number;
    connectionContainer: ConnectionContainer;
}
