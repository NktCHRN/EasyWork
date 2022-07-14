import { TagModel } from "../../tag/tag.model";

export interface ProjectTagDeletePageModel {
    tag: TagModel;
    projectId: number;
}
