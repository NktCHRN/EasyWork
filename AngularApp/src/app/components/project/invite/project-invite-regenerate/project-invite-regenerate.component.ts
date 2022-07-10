import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';

@Component({
  selector: 'app-project-invite-regenerate',
  templateUrl: './project-invite-regenerate.component.html',
  styleUrls: ['./project-invite-regenerate.component.scss']
})
export class ProjectInviteRegenerateComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  private _projectId: number;
  @Output() inviteCodeChange = new EventEmitter<string>();

  constructor(private _dialogRef: MatDialogRef<ProjectInviteRegenerateComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: number, private _projectService: ProjectService, 
    private _tokenService: TokenService) {
    this._projectId = data
  }

  ngOnInit(): void {
  }

  onSubmit() : void {
    this.loading = true;
    this._projectService.regenerateInviteCode(this._tokenService.getJwtToken()!, this._projectId).subscribe(
      {
        next: result => {
          this.success = true;
          this.loading = false;
          this.inviteCodeChange.emit(result);
          this._dialogRef.close();
        },
        error: error => {
          this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          this.loading = false;
        }
      }
    );
  }
}
