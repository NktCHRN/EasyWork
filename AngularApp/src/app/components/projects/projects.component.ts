import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ProjectService } from '../../services/project.service';
import { ProjectReducedModel } from '../../shared/project/project-reduced.model';
import { ProjectAddComponent } from './project-add/project-add.component';
import * as signalR from '@microsoft/signalr';
import { TokenService } from 'src/app/services/token.service';
import { ProjectModel } from 'src/app/shared/project/project.model';
import { UpdateProjectModel } from 'src/app/shared/project/update-project.model';
import { UserOnProjectModel } from 'src/app/shared/project/user-on-project/user-on-project.model';
import { TokenGuardService } from 'src/app/services/token-guard.service';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit {

  projects: ProjectReducedModel[] | null | undefined;
  errorMessage: string | null | undefined;
  loading: boolean = true;

  connection: signalR.HubConnection | null | undefined;
  readonly myId: number;

  constructor(private _projectsService: ProjectService, private _dialog: MatDialog, @Inject('signalRURL') private _signalRURL: string,
  private _tokenService: TokenService, private _tokenGuardService: TokenGuardService) {
    this.myId = this._tokenService.getMyId()!;
   }

  ngOnInit(): void {
    const connectPromise = this.connect();
    this._projectsService.get()
    .subscribe({
      next: projects => 
      {
        this.projects = projects; 
        this.loading = false;
        connectPromise.then(() => {
          this.addProjects();
        });
      },
      error: error => 
      {
        this.errorMessage = typeof error === 'string' || error instanceof String ? error : error.message; 
        this.loading = false;
      },
    });
  }

  private async connect(): Promise<void> {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this._signalRURL + "projectsHub", {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
      })
      .withAutomaticReconnect()
      .build();
      this.connection?.onreconnected(() => this.addProjects());
      this.connection.on("Added", (data: ProjectModel) => {
        this.projects?.unshift(data);
        this.addProject(data.id);
      });
      this.connection.on("Updated", (id: number, model: UpdateProjectModel) => {
        const found = this.projects?.find(p => p.id == id);
        if (found)
        {
          found.name = model.name;
          found.description = model.description;
        }
      });
      this.connection.on("Deleted", (id: number) => {
        const foundIndex = this.projects?.findIndex(p => p.id == id);
        if (foundIndex != undefined && foundIndex != null && foundIndex != -1)
          this.projects?.splice(foundIndex, 1);
        this.deleteProject(id);
      });
      this.connection.on("DeletedUser", (projectId: number, userId: number) => {
        if (userId == this.myId)
        {
          const foundIndex = this.projects?.findIndex(p => p.id == projectId);
          if (foundIndex != undefined && foundIndex != null && foundIndex != -1)
            this.projects?.splice(foundIndex, 1);
          this.deleteProject(projectId);
        }
      });
      this.connection.on("AddedUser", (data: UserOnProjectModel) => {
        if (data.userId == this.myId)
          this._projectsService.getReducedById(data.projectId).subscribe(result => {
            this.projects?.unshift(result);
            this.addProject(result.id);
          });
      });
      try {
      return await this.connection.start();
    } catch (err) {
      return console.error(err);
    }
  }

  private addProjects() {
    this.projects?.forEach(p => this.addProject(p.id));
  }

  private addProject(id: number) {
    this.connection!.invoke('StartListening', id)
    .catch(error => console.error(error))
  }

  private deleteProject(id: number) {
    this.connection!.invoke('StopListening', id)
    .catch(error => console.error(error))
  }

  onAddClick()
  {
    this._dialog.open(ProjectAddComponent, {
      panelClass: "dialog-responsive"
    });
  }

  ngOnDestroy()
  {
    if (this.connection && this.connection.state == signalR.HubConnectionState.Connected)
      this.connection.stop().then(() => this.connection = null);
    else
      this.connection = null!
  }
}
