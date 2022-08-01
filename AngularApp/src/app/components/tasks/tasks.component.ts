import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TokenService } from 'src/app/services/token.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskService } from '../../services/task.service';
import { UserTaskModel } from '../../shared/task/user-task.model';
import { TaskComponent } from '../project/project-tasks/task/task.component';
import * as signalR from '@microsoft/signalr';
import { TokenGuardService } from 'src/app/services/token-guard.service';
import { UpdateTaskModel } from 'src/app/shared/task/update-task.model';

@Component({
  selector: 'app-tasks',
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.scss']
})
export class TasksComponent implements OnInit, OnDestroy {
  tasks: UserTaskModel[] | null | undefined;
  errorMessage: string | null | undefined;
  loading: boolean = true;
  readonly statuses: Map<string, string>;
  private readonly _myId: number;

  projectsConnectionContainer: ConnectionContainer = new ConnectionContainer();
  connectionContainer: ConnectionContainer = new ConnectionContainer();
  usersConnection: signalR.HubConnection;

  constructor(private _tasksService: TaskService, private _dialog: MatDialog, private _tokenService: TokenService,
    @Inject('signalRURL') private _signalRURL: string, private _tokenGuardService: TokenGuardService) {
     this.statuses = new Map(
      _tasksService.getStatusesWithDescriptions(true).map(object => {
        return [object.status.toString(), object.description];
      }),
    );
    this._myId = this._tokenService.getMyId()!;
    this.usersConnection = new signalR.HubConnectionBuilder()
          .withUrl(this._signalRURL + "usersHub", {
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets,
            accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
          })
          .withAutomaticReconnect()
          .build();
    this.connectionContainer.connection = new signalR.HubConnectionBuilder()
          .withUrl(this._signalRURL + "tasksHub", {
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets,
            accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
          })
          .withAutomaticReconnect()
          .build();
    this.projectsConnectionContainer.connection = new signalR.HubConnectionBuilder()
      .withUrl(this._signalRURL + "projectsHub", {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
      })
      .withAutomaticReconnect()
      .build();
   }

  async ngOnInit(): Promise<void> {
    this._tasksService.get()
    .subscribe({
      next: async tasks => 
      {
        this.tasks = tasks; 
        this.loading = false;
        this.projectsConnectionContainer.connection.on("Deleted", (id: number) => {
          this.deleteAllProjectTasks(id);
        });
        this.projectsConnectionContainer.connection.on("DeletedUser", (projectId: number, userId: number) => {
          if (userId == this._myId)
            this.deleteAllProjectTasks(projectId);
        });
        this.projectsConnectionContainer.connection.on("DeletedTask", (_, taskId: number) => {
          this.deleteTaskById(taskId);
        });

        this.connectionContainer.connection.on("Updated", (taskId: number, model: UpdateTaskModel) => {
          this.updateTask(taskId, model);
        });
        this.connectionContainer.connection.on("DeletedExecutor", (taskId: number, executorId: number) => {
          if (executorId == this._myId)
            this.deleteTaskByIdWithoutUnsubscribe(taskId);
        });
    
        this.usersConnection.on("AddedAsExecutor", (model: UserTaskModel) =>
        {
          if (this.tasks?.findIndex(t => t.id == model.id) == -1)
            this._tasksService.insertUserTask(model, this.tasks!);
          this.subscribeToTask(model.id);
          this.subscribeToProject(model.projectId);
        });
        try {
          await this.usersConnection.start();
        } catch (err) {
          console.error(err);
        }
          this.containers.forEach(async c =>
            {
              c.connection.on("ConnectionId", (result: string | null) => 
                {
                  c.id = result;
                });
              c.connection.onreconnected(() => {
                this.getConnectionId(c);
                if (c.connection.baseUrl.endsWith("tasksHub"))
                  this.subscribeToAll();
                else if (c.connection.baseUrl.endsWith("projectsHub"))
                  this.subscribeToAllProjects();
              });
              try {
                await c.connection.start().then(() => {
                  this.getConnectionId(c);
                  if (c.connection.baseUrl.endsWith("tasksHub"))
                    this.subscribeToAll();
                  else if (c.connection.baseUrl.endsWith("projectsHub"))
                    this.subscribeToAllProjects();
                  });
              } catch (err) {
                console.error(err);
              }
            });
      },
      error: error => 
      {
        this.errorMessage = typeof error === 'string' || error instanceof String ? error : error.message; 
        this.loading = false;
      },
    });
  }

  private deleteTaskById(id: number)
  {
    this.unsubscribeFromTask(id);
    this.deleteTaskByIdWithoutUnsubscribe(id);
  }

  private deleteTaskByIdWithoutUnsubscribe(id: number)
  {
    let found = this.tasks?.findIndex(t => t.id == id);
    if (found != undefined && found != null && found != -1)
      this.tasks?.splice(found, 1);
  }

  private deleteAllProjectTasks(projectId: number)
  {
    if (!this.tasks)
      return;
    let foundIndex = this.tasks!.findIndex(t => t.projectId == projectId);
    while(foundIndex != -1) {
      this.unsubscribeFromTask(this.tasks[foundIndex].id);
      this.tasks.splice(foundIndex, 1);
      foundIndex = this.tasks!.findIndex(t => t.projectId == projectId);
    };
    this.unsubscribeFromProject(projectId);
  }

  private subscribeToAllProjects(): void
  {
    if (!this.tasks)
      return;
    let projects: number[] = [];
    this.tasks.forEach(t =>
      {
        if (projects.findIndex(p => p == t.projectId) == -1)
          projects.push(t.projectId)
      });
    projects.forEach(p => this.subscribeToProject(p));
  }

  private subscribeToProject(id: number): void
  {
    this.projectsConnectionContainer.connection.invoke('StartListening', id)
    .catch(error => console.error(error));
  }

  private unsubscribeFromProject(id: number): void
  {
    this.projectsConnectionContainer.connection.invoke('StopListening', id)
    .catch(error => console.error(error));
  }

  private subscribeToAll(): void
  {
    if (!this.tasks)
      return;
    this.tasks.forEach(t => this.subscribeToTask(t.id));
  }

  private subscribeToTask(id: number): void
  {
    this.connectionContainer.connection.invoke('StartListening', id)
    .catch(error => console.error(error));
  }

  private unsubscribeFromTask(id: number): void
  {
    this.connectionContainer.connection.invoke('StopListening', id)
    .catch(error => console.error(error));
  }

  private getConnectionId(container: ConnectionContainer)
  {
   container.connection.invoke('GetConnectionId')
    .catch(error => console.error(error));
  }

  private updateTask(id: number, task: UpdateTaskModel): void {
    const foundTask = this.tasks?.find(t => t.id == id);
    if (foundTask)
    {
      const oldStatus = foundTask?.status;
      foundTask.name = task.name;
      foundTask.deadline = task.deadline;
      foundTask.endDate = task.endDate;
      foundTask.status = task.status;
      if (this._tasksService.isDone(oldStatus as TaskStatus) != this._tasksService.isDone(task.status))
      {
        const index = this.tasks!.indexOf(foundTask);
        if (index != -1)
        {
          this.tasks?.splice(index, 1);
          this._tasksService.insertUserTask(foundTask, this.tasks!);
        }
      }
    }
  }

  openDialog(id: number): void {
    const dialogRef = this._dialog.open(TaskComponent, {
      panelClass: "dialog-responsive",
      data: {
        taskId: id,
        showToProjectButton: true,
        connectionContainer: this.connectionContainer,
        projectsConnectionContainer: this.projectsConnectionContainer
      }
    });
    dialogRef.componentInstance.updatedTask.subscribe(
      task => {
        this.updateTask(id, task);
      }
    );
    dialogRef.componentInstance.deletedTask.subscribe(
      () => {
        this.deleteTaskById(id);
      }
    );
    dialogRef.componentInstance.deletedExecutor.subscribe(
      userId => {
        if (userId == this._myId)
          this.deleteTaskByIdWithoutUnsubscribe(id);
      }
    );
  }

  private get containers() {
    return [this.connectionContainer, this.projectsConnectionContainer];
  }

  ngOnDestroy(): void {
    if (this.usersConnection && this.usersConnection.state == signalR.HubConnectionState.Connected)
      this.usersConnection.stop().then(() => this.usersConnection = null!);
    else
      this.usersConnection = null!
    const containers = this.containers;
    containers.forEach(c => {
      if (c.connection && c.connection.state == signalR.HubConnectionState.Connected)
        c.connection.stop().then(() => c.connection = null!);
      else
        c.connection = null!
    });
  }
}
