import { FileLoadingParameters } from "./file-loading-parameters";
import { FileReducedModel } from "./file-reduced.model";

export class FileModel extends FileReducedModel {
    size: number | null | undefined;
    loadingParameters: FileLoadingParameters | null | undefined;
}
