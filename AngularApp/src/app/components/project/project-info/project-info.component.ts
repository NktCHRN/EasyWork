import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { ProjectsService } from 'src/app/services/projects.service';
import { TokenService } from 'src/app/services/token.service';
import { ProjectInfoMode } from 'src/app/shared/project/info/project-info-mode';
import { ProjectModel } from 'src/app/shared/project/project.model';
import { UserOnProjectRole } from 'src/app/shared/project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { ErrorDialogComponent } from '../../error-dialog/error-dialog.component';

@Component({
  selector: 'app-project-info',
  templateUrl: './project-info.component.html',
  styleUrls: ['./project-info.component.scss']
})
export class ProjectInfoComponent implements OnInit {
  project: ProjectModel = undefined!;
  projectId: number = undefined!;
  projectName: string = undefined!;
  userOnProjectRoles = UserOnProjectRole;
  me: UserOnProjectReducedModel = undefined!;
  loading: boolean = true;
  pageMode: ProjectInfoMode = ProjectInfoMode.Show;
  pageModes = ProjectInfoMode;
  // add a project reduced (name&description) change emitter and a project change emitter to child components

  constructor(private _tokenService: TokenService, private _titleService: Title, 
    private _projectService: ProjectsService, @Inject('projectName') private _websiteName: string, private _router: Router, 
    private _dialog: MatDialog) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Info - ${this._websiteName}`);
      this._projectService.getById(this._tokenService.getJwtToken()!, this.projectId)
      .subscribe({
        next: result => 
        {this.loading = false; this.project = result; },
        error: error => 
        {
          this.loading = false;
          if (error.status == 404 || error.status == 400)
            this._router.navigate(["**"], { skipLocationChange: true });
          else
            this._dialog.open(ErrorDialogComponent, {
              panelClass: "dialog-responsive",
              data: JSON.stringify(error)
            });
        }
      });
  }

  onModeChange(): void
  {
    if (this.pageMode == ProjectInfoMode.Show)
      this.pageMode = ProjectInfoMode.Edit;
    else
      this.pageMode = ProjectInfoMode.Show
  }
}
