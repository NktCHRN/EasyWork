import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { ProjectRoleService } from 'src/app/services/project-role.service';
import { ProjectService } from 'src/app/services/project.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { ProjectInfoMode } from 'src/app/shared/project/info/project-info-mode';
import { ProjectModel } from 'src/app/shared/project/project.model';
import { UpdateProjectModel } from 'src/app/shared/project/update-project.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { UserOnProjectModel } from 'src/app/shared/project/user-on-project/user-on-project.model';
import { ErrorDialogComponent } from '../../error-dialog/error-dialog.component';
import { ProjectInfoDeleteComponent } from './project-info-delete/project-info-delete.component';

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
  @Output() projectNameChange = new EventEmitter<string>();

  connectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _titleService: Title, 
    private _projectService: ProjectService, @Inject('projectName') private _websiteName: string, private _router: Router, 
    private _dialog: MatDialog, private _projectRoleService: ProjectRoleService) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Info - ${this._websiteName}`);
      this._projectService.getById(this.projectId)
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
      this.connectionContainer.connection.on("UpdatedUser", (model: UserOnProjectModel) => {
        if (model.userId == this.me.userId && model.projectId == this.projectId && this._projectRoleService.roleToEnum(model.role) < UserOnProjectRole.Owner)
          this.pageMode = this.pageModes.Show;
      });
      this.connectionContainer.connection.on("Updated", (id: number, model: UpdateProjectModel) => {
        if (id == this.projectId)
        {
          this.projectName = model.name;
          this.project.name = model.name;
          this.project.description = model.description;
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

  onProjectNameChange(name: string): void 
  {
    this.projectName = name;
    this._titleService.setTitle(`${name} | Info - ${this._websiteName}`);
    this.projectNameChange.emit(this.projectName);
  }

  openDeleteDialog(): void {
    this._dialog.open(ProjectInfoDeleteComponent, {
      panelClass: "dialog-responsive",
      data: {
        id: this.projectId,
        name: this.projectName
      }
    });
  }
}
