import { Component, Inject, OnDestroy, OnInit, QueryList, ViewChildren } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { ProjectService } from 'src/app/services/project.service';
import { TaskService } from 'src/app/services/task.service';
import { TokenGuardService } from 'src/app/services/token-guard.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { ProjectLimitsModel } from 'src/app/shared/project/limits/project-limits.model';
import { TasksCountModel } from 'src/app/shared/project/tasks/tasks-count.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskStatusChangeModel } from 'src/app/shared/task/status/task-status-change.model';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';
import { TaskReducedComponent } from '../project-tasks/task-reduced/task-reduced.component';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-project-archive',
  templateUrl: './project-archive.component.html',
  styleUrls: ['./project-archive.component.scss']
})
export class ProjectArchiveComponent implements OnInit, OnDestroy {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;
  tasks: TaskReducedModel[] = undefined!;
  loadError: boolean = false;
  @ViewChildren(TaskReducedComponent) viewTasks: QueryList<TaskReducedComponent> = undefined!;
  taskStatuses = TaskStatus;
  limits: ProjectLimitsModel = {
    maxToDo: undefined!,
    maxInProgress: undefined!,
    maxValidate: undefined!
  };
  tasksCount: TasksCountModel = {
    toDo: undefined!,
    inProgress: undefined!,
    validate: undefined!,
    done: undefined!
  }

  connectionContainer: ConnectionContainer = new ConnectionContainer();

  tasksConnectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string,
  private _projectService: ProjectService, private _snackBar: MatSnackBar,
  private _taskService: TaskService, @Inject('signalRURL') private _signalRURL: string,
  private _tokenGuardService: TokenGuardService) {
    this.tasksConnectionContainer.connection = new signalR.HubConnectionBuilder()
    .withUrl(this._signalRURL + "tasksHub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
    })
    .withAutomaticReconnect()
    .build();
   }

  async ngOnInit(): Promise<void> {
    this._titleService.setTitle(`${this.projectName} | Archive - ${this._websiteName}`);
    this._projectService.getArchivedTasks(this.projectId)
    .subscribe({
      next: result => {
        this.tasks = result;
      },
      error: error => {
        this.loadError = true;
        this._snackBar.open("Tasks have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
    this._projectService.getLimits(this.projectId)
    .subscribe({
      next: result => 
      {
        for (let key in result)
        {
          const objectKey = key as keyof typeof result;
          this.limits[objectKey] = result[objectKey];
        }
      },
      error: error => 
      this._snackBar.open("Max quantities have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
    });
    this._projectService.getTasks(this.projectId)
    .subscribe({
      next: result => 
      {
        for (let key in result)
          this.tasksCount[key as keyof TasksCountModel] = result[key as keyof typeof result].length;
      },
      error: error => 
        this._snackBar.open("Tasks have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
    });
    this.connectionContainer.connection.on("DeletedTask", (id: number, taskId: number) => {
      if (id == this.projectId)
        this.onDeletedTask(taskId);
    });
    this.connectionContainer.connection.on("TaskStatusChanged", (id: number, model: TaskStatusChangeModel) => {
      if (id == this.projectId)
      {
        if (model.old == TaskStatus.Archived)
          this.onMovedFromArchive(model.id);
        else if (model.new == TaskStatus.Archived)
        {
          this._taskService.getReducedById(model.id)
          .subscribe({
            next: result => this.onMovedToArchive(result),
            error: error => console.error(error)
          });
        }
      }
    });

    this.tasksConnectionContainer.connection.on("ConnectionId", (result: string | null) => 
    {
      this.tasksConnectionContainer.id = result;
    })
    this.tasksConnectionContainer.connection.onreconnected(() => this.getTasksConnectionId());
    try {
      return await this.tasksConnectionContainer.connection.start().then(() => this.getTasksConnectionId());
    } catch (err) {
      return console.error(err);
    }
  }

  private getTasksConnectionId(): void {
    this.tasksConnectionContainer.connection.invoke('GetConnectionId')
      .catch(error => console.error(error));
  }

  onMovedToArchive(event: TaskReducedModel): void {
    this.tasks.splice(this._taskService.getInsertAtIndexByTaskId(event.id,  this.tasks), 0, event);
    this.subscribeToTask(event.id);
  }

  onMovedFromArchive(id: number): void {
    this.onDeletedTask(id);
    this.subscribeToTask(id);
  }

  onDeletedTask(id: number): void {
    const index = this.tasks.findIndex(t => t.id == id);
    if (index != -1)
      this.tasks.splice(index, 1);
  }

  private subscribeToTask(taskId: number) {
    const foundViewTask = this.viewTasks.find(t => t.model.id == taskId);
    foundViewTask?.movedToArchived.subscribe(m => this.onMovedToArchive(m));
    foundViewTask?.movedFromArchived.subscribe(m => this.onMovedFromArchive(m.task.id));
  }

  ngOnDestroy()
  {
    if (this.tasksConnectionContainer.connection && this.tasksConnectionContainer.connection.state == signalR.HubConnectionState.Connected)
      this.tasksConnectionContainer.connection.stop().then(() => this.tasksConnectionContainer = null!);
    else
      this.tasksConnectionContainer = null!
  }
}
