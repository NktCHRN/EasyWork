import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { ProjectService } from 'src/app/services/project.service';
import { ProjectMiniModel } from 'src/app/shared/project/project-mini.model';

@Component({
  selector: 'app-project-leave',
  templateUrl: './project-leave.component.html',
  styleUrls: ['./project-leave.component.scss']
})
export class ProjectLeaveComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  private _projectId: number;
  projectName: string;

  constructor(private _dialogRef: MatDialogRef<ProjectLeaveComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: ProjectMiniModel, private _router: Router,
    private _projectService: ProjectService) {
      this._projectId = data.id;
      this.projectName = data.name;
      this._dialogRef.afterClosed()
      .subscribe(() => {
        if (this.success)
          this._router.navigate(['projects']);
      });
  }

  ngOnInit(): void {
  }

  close(): void {
    if (!this._dialogRef.disableClose)
      this._dialogRef.close();
  }

  onSubmit() : void {
    this.loading = true;
    this._dialogRef.disableClose = true;
      this._projectService.leave(this._projectId).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
          this._dialogRef.disableClose = false;
        },
        error: error => {
          this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          this.loading = false;
          this._dialogRef.disableClose = false;
        }
      }
      );
  }
}
