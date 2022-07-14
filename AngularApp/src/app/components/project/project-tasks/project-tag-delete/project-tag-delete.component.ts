import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { ProjectTagDeletePageModel } from 'src/app/shared/project/tag/project-tag-delete-page.model';
import { TagModel } from 'src/app/shared/tag/tag.model';

@Component({
  selector: 'app-project-tag-delete',
  templateUrl: './project-tag-delete.component.html',
  styleUrls: ['./project-tag-delete.component.scss']
})
export class ProjectTagDeleteComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  model: TagModel;
  private _projectId: number;

  constructor(private _dialogRef: MatDialogRef<ProjectTagDeleteComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: ProjectTagDeletePageModel,
    private _projectService: ProjectService, private _tokenService: TokenService) {
      this.model = data.tag;
      this._projectId = data.projectId
  }

  ngOnInit(): void {
  }

  onSubmit() : void {
    this.loading = true;
      this._projectService.deleteTag(this._tokenService.getJwtToken()!, this._projectId, this.model.id).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
        },
        error: error => {
          this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          this.loading = false;
        }
      }
      );
  }
}
