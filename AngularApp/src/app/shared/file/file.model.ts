import { FileReducedModel } from "./file-reduced.model";

export interface FileModel extends FileReducedModel {
    size: number | null | undefined;
}
