import { ConnectionContainer } from "../other/connection-container";
import { FileModel } from "./file.model";

export interface DeleteFilePageModel {
    model: FileModel;
    tasksConnectionContainer: ConnectionContainer;
}
