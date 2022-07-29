import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectService } from 'src/app/services/project.service';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';
import * as signalR from '@microsoft/signalr';
import { TokenGuardService } from 'src/app/services/token-guard.service';
import { UpdateProjectModel } from 'src/app/shared/project/update-project.model';
import { UserOnProjectModel } from 'src/app/shared/project/user-on-project/user-on-project.model';
import { ProjectRoleService } from 'src/app/services/project-role.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';

@Component({
  selector: 'app-project',
  templateUrl: './project.component.html',
  styleUrls: ['./project.component.scss']
})
export class ProjectComponent implements OnInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;

  connectionContainer: ConnectionContainer = new ConnectionContainer();
  
  constructor(private _route: ActivatedRoute, private _projectService: ProjectService, private _dialog: MatDialog, 
    @Inject('signalRURL') private _signalRURL: string, private _tokenGuardService: TokenGuardService, private _router: Router,
    private _projectRoleService: ProjectRoleService) {
    this.connectionContainer.connection = new signalR.HubConnectionBuilder()
    .withUrl(this._signalRURL + "projectsHub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
    })
    .withAutomaticReconnect()
    .build();
   }

  ngOnInit(): void {
    this._route.paramMap.subscribe(async params => {
      this.projectId = parseInt(params.get('id')!);
      this._projectService.getReducedById(this.projectId)
      .subscribe({
        next: result => this.projectName = result.name,
        error: () => this.projectName = `Project ${this.projectId}`
      });
      this._projectService.getMeAsProjectUser(this.projectId)
      .subscribe({
        next: result => this.me = result,
        error: error => {
          this._dialog.open(ErrorDialogComponent, {
            panelClass: "dialog-responsive",
            data: JSON.stringify(error) + '\nPlease, reload the page'
          });
        }
      });
      this.connectionContainer.connection.onreconnected(() => this.addProject());
      this.connectionContainer.connection.on("Updated", (id: number, model: UpdateProjectModel) => {
        if (id == this.projectId)
          this.projectName = model.name;
      });
      this.connectionContainer.connection.on("Deleted", (id: number) => {
        if (id == this.projectId)
          this._router.navigate(['projects']);
      });
      this.connectionContainer.connection.on("DeletedUser", (projectId: number, userId: number) => {
        if (userId == this.me.userId && projectId == this.projectId)
          this._router.navigate(['projects']);
      });
      this.connectionContainer.connection.on("UpdatedUser", (model: UserOnProjectModel) => {
        if (model.userId == this.me.userId && model.projectId == this.projectId)
          this.me.role = this._projectRoleService.roleToEnum(model.role);
      });
      this.connectionContainer.connection.on("ConnectionId", (result: string | null) => 
      {
        this.connectionContainer.id = result;
      })
      try {
      return await this.connectionContainer.connection.start().then(() => {
          this.addProject();
        });
    } catch (err) {
      return console.error(err);
    }
    });
  }

  private addProject() {
    this.connectionContainer.connection.invoke('StartListening', this.projectId)
      .catch(error => console.error(error));
    this.connectionContainer.connection.invoke('GetConnectionId')
      .catch(error => console.error(error));
  }

  onOutletLoaded(component: any): void {
    component.projectId = this.projectId;
    component.projectName = this.projectName;
    component.me = this.me;
    component.userOnProjectRoles = this.userOnProjectRoles;
    component.projectNameChange?.subscribe((result: string) => this.onProjectNameChange(result));
    component.connectionContainer = this.connectionContainer;
  } 

  onProjectNameChange(name: string): void 
  {
    this.projectName = name;
  }

  ngOnDestroy()
  {
    if (this.connectionContainer.connection && this.connectionContainer.connection.state == signalR.HubConnectionState.Connected)
      this.connectionContainer.connection.stop().then(() => this.connectionContainer = null!);
    else
      this.connectionContainer = null!
  }
}
