import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TaskService } from 'src/app/services/task.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { ProjectLimitsModel } from 'src/app/shared/project/limits/project-limits.model';
import { TasksCountModel } from 'src/app/shared/project/tasks/tasks-count.model';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskStatusChangeModel } from 'src/app/shared/task/status/task-status-change.model';
import { TaskReducedWithStatusModel } from 'src/app/shared/task/task-reduced-with-status.model';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';
import { UserMiniWithAvatarModel } from 'src/app/shared/user/user-mini-with-avatar.model';
import { TaskComponent } from '../task/task.component';
import * as signalR from '@microsoft/signalr';
import { UpdateTaskModel } from 'src/app/shared/task/update-task.model';
import { ConnectionService } from 'src/app/services/connection.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-task-reduced',
  templateUrl: './task-reduced.component.html',
  styleUrls: ['./task-reduced.component.scss']
})
export class TaskReducedComponent implements OnInit, OnDestroy {
  @Input() model: TaskReducedModel = undefined!;
  @Input() status: TaskStatus = undefined!
  executors: UserMiniWithAvatarModel[] = [];
  prioritiesWithColors: any;
  @Output() updatedStatus: EventEmitter<TaskStatusChangeModel> = new EventEmitter<TaskStatusChangeModel>();
  @Output() movedFromArchived: EventEmitter<TaskReducedWithStatusModel> = new EventEmitter<TaskReducedWithStatusModel>();
  @Output() movedToArchived: EventEmitter<TaskReducedModel> = new EventEmitter<TaskReducedModel>();
  @Output() deleted: EventEmitter<number> = new EventEmitter<number>();
  @Input() limits: ProjectLimitsModel = undefined!;
  @Input() tasksCount: TasksCountModel = undefined!;

  @Input() projectsConnectionContainer: ConnectionContainer = new ConnectionContainer();
  @Input() connectionContainer: ConnectionContainer = new ConnectionContainer();

  turnedOffConnection: boolean = false;

  constructor(private _taskService: TaskService, private _dialog: MatDialog, private _connectionService: ConnectionService,
    private _userService: UserService) { }

  ngOnInit(): void {
    this.prioritiesWithColors = this._taskService.getPrioritiesWithColors();
    this._taskService.getExecutors(this.model.id)
    .subscribe(result => this.executors = result);
    this.connectionContainer.connection.onreconnected(() => this.startListening());
    this.connectionContainer.connection.on("Updated", (taskId: number, model: UpdateTaskModel) => {
      if (taskId == this.model.id && !this.turnedOffConnection)
        this.updateTask(model);
    });
    this.connectionContainer.connection.on("AddedExecutor", (taskId: number, executorId: number) => {
      if (taskId == this.model.id && !this.turnedOffConnection)
        this._userService.getById(executorId)
        .subscribe({
          next: result => this.addExecutor({
            id: executorId,
            fullName: this._userService.getFullName(result.firstName, result.lastName),
            ...result
          }),
          error: error => console.error(error)
        });
    });
    this.connectionContainer.connection.on("DeletedExecutor", (taskId: number, executorId: number) => {
      if (taskId == this.model.id && !this.turnedOffConnection)
        this.deleteExecutor(executorId);
    });
    this.connectionContainer.connection.on("StartedFileUpload", (taskId: number, _) =>
    {
      if (taskId == this.model.id && !this.turnedOffConnection)
        this.model.filesCount++;
    });
    this.connectionContainer.connection.on("UploadedFile", (taskId: number, _) =>   // deprecated
    {
      if (taskId == this.model.id && !this.turnedOffConnection)
        this.model.filesCount++;
    });
    this.connectionContainer.connection.on("DeletedFile", (taskId: number, _) =>
    {
      if (taskId == this.model.id && !this.turnedOffConnection)
        this.model.filesCount--;
    });
    this.connectionContainer.connection.on("AddedMessage", (taskId: number, _) => {
      if (taskId == this.model.id && !this.turnedOffConnection)
        this.model.messagesCount++;
    });
    this.connectionContainer.connection.on("DeletedMessage", (taskId: number, _) =>
    {
      if (taskId == this.model.id && !this.turnedOffConnection)
        this.model.messagesCount--;
    });
    this._connectionService.getOpenConnection(this.connectionContainer.connection)
    .then(() => this.startListening())
    .catch(() => console.error("An error occured while waiting for connection to be in the \"Connected\" state"));
  }

  private updateTask(model: UpdateTaskModel): void {
    this.model.deadline = model.deadline;
    this.model.endDate = model.endDate;
    this.model.name = model.name;
    this.model.priority = model.priority;
    this.status = model.status;
  }

  openDialog()
  {
    const dialogRef = this._dialog.open(TaskComponent, {
      panelClass: "dialog-responsive",
      data: {
        taskId: this.model.id,
        showToProjectButton: false,
        tasksCount: this.tasksCount,
        limits: this.limits,
        projectsConnectionContainer: this.projectsConnectionContainer,
        connectionContainer: this.connectionContainer
      }
    });
    dialogRef.componentInstance.updatedTask.subscribe(
      task => {
        if (this.status != task.status)
        {
          if (this.status == TaskStatus.Archived)
            this.movedFromArchived.emit({
              task: this.model,
              status: task.status
            });
          if (task.status == TaskStatus.Archived)
            this.movedToArchived.emit(this.model);
          this.updatedStatus.emit({
            old: this.status,
            new: task.status,
            id: this.model.id
          });
        }
        this.updateTask(task);
        console.log(this.model);
      }
    );
    dialogRef.componentInstance.deletedTask.subscribe(
      id =>
      { 
        this.deleted.emit(id);
        if (this.connectionContainer.connection && this.connectionContainer.connection.state == signalR.HubConnectionState.Connected)
        this.connectionContainer.connection.invoke('StopListening', this.model.id)
        .catch(error => console.error(error));
      }
    );
    dialogRef.componentInstance.addedMessage.subscribe(() => this.model.messagesCount++);
    dialogRef.componentInstance.deletedMessage.subscribe(() => this.model.messagesCount--);

    dialogRef.componentInstance.deletedExecutor.subscribe(id => this.deleteExecutor(id));
    dialogRef.componentInstance.addedExecutor.subscribe(e => this.addExecutor(e));

    dialogRef.componentInstance.deletedFile.subscribe(() => this.model.filesCount--);
    dialogRef.componentInstance.addedFile.subscribe(() => this.model.filesCount++);
  }

  private addExecutor(executor: UserMiniWithAvatarModel): void
  {
    this.executors.push(executor);
  }

  private deleteExecutor(id: number): void {
    const index = this.executors.findIndex(e => e.id == id);
    if (index != -1)
      this.executors.splice(index, 1);
  }

  private startListening(): void
  {
    this.connectionContainer.connection.invoke('StartListening', this.model.id)
    .catch(error => console.error(error));
  }

  ngOnDestroy()
  {
    this.turnedOffConnection = true;
  }
}
