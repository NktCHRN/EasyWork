import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { ProjectKickPageModel } from 'src/app/shared/project/user-on-project/kick/project-kick-page.model';
import { UserMiniReducedModel } from 'src/app/shared/user/user-mini-reduced.model';

@Component({
  selector: 'app-project-kick',
  templateUrl: './project-kick.component.html',
  styleUrls: ['./project-kick.component.scss']
})
export class ProjectKickComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  private _projectId: number;
  projectName: string;
  toKick: UserMiniReducedModel;

  constructor(private _dialogRef: MatDialogRef<ProjectKickComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: ProjectKickPageModel,
    private _projectService: ProjectService, private _tokenService: TokenService) { 
      this._projectId = data.project.id;
      this.projectName = data.project.name;
      this.toKick = data.toKick;
  }

  ngOnInit(): void {
  }

  onSubmit()
  {
    this.loading = true;
      this._projectService.kick(this._tokenService.getJwtToken()!, {
        id: this._projectId, 
        userId: this.toKick.id
      }).subscribe(
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
