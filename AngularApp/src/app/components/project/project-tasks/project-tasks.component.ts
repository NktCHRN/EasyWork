import { Component, Inject, OnDestroy, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { createNumberOrUnlimitedValidator } from 'src/app/customvalidators';
import { ProjectRoleService } from 'src/app/services/project-role.service';
import { ProjectService } from 'src/app/services/project.service';
import { TaskService } from 'src/app/services/task.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { ProjectLimitsModel } from 'src/app/shared/project/limits/project-limits.model';
import { TasksCountModel } from 'src/app/shared/project/tasks/tasks-count.model';
import { TasksModel } from 'src/app/shared/project/tasks/tasks.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { UserOnProjectModel } from 'src/app/shared/project/user-on-project/user-on-project.model';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskStatusChangeModel } from 'src/app/shared/task/status/task-status-change.model';
import { TaskStatusWithDescriptionModel } from 'src/app/shared/task/status/task-status-with-description.model';
import { TaskReducedWithStatusModel } from 'src/app/shared/task/task-reduced-with-status.model';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';
import { TaskModel } from 'src/app/shared/task/task.model';
import { UserMiniWithAvatarModel } from 'src/app/shared/user/user-mini-with-avatar.model';
import { TaskReducedComponent } from './task-reduced/task-reduced.component';
import * as signalR from '@microsoft/signalr';
import { TokenGuardService } from 'src/app/services/token-guard.service';

@Component({
  selector: 'app-project-tasks',
  templateUrl: './project-tasks.component.html',
  styleUrls: ['./project-tasks.component.scss']
})
export class ProjectTasksComponent implements OnInit, OnDestroy {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;
  limits: ProjectLimitsModel = undefined!;
  tasks: TasksModel = {
    toDo: undefined!,
    inProgress: undefined!,
    validate: undefined!,
    done: undefined!
  };
  tasksCount: TasksCountModel = {
    toDo: undefined!,
    inProgress: undefined!,
    validate: undefined!,
    done: undefined!
  }
  loadError: boolean = false;
  taskStatuses = TaskStatus;
  selectedStatus: TaskStatus = TaskStatus.ToDo;
  readonly statusesWithDescription: TaskStatusWithDescriptionModel[];

  form: FormGroup = null!;
  @ViewChild('lform') formDirective: any;
  @ViewChildren(TaskReducedComponent) viewTasks: QueryList<TaskReducedComponent> = undefined!;

  formErrors : any = {
    'toDo': '',
    'inProgress': '',
    'validate': ''
  };

  validationMessages : any = {
    'toDo': {
      'required':      'Max quantity is required.',
      'numberOrUnlimited': 'Max quantity must be a positive number, a zero or "unlimited"'
    },
    'inProgress': {
      'required':      'Max quantity is required.',
      'numberOrUnlimited': 'Max quantity must be a positive number, a zero or "unlimited"'
    },
    'validate': {
      'required':      'Max quantity is required.',
      'numberOrUnlimited': 'Max quantity must be a positive number, a zero or "unlimited"'
    }
  };

  toDoControl: AbstractControl = undefined!;
  inProgressControl: AbstractControl = undefined!;
  validateControl: AbstractControl = undefined!;

  get controls(){
    return [this.toDoControl, this.inProgressControl, this.validateControl];
  }

  connectionContainer: ConnectionContainer = new ConnectionContainer();

  usersOnPage: UserMiniWithAvatarModel[] = undefined!;

  tasksConnectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string, 
  private _fb: FormBuilder, private _snackBar: MatSnackBar, private _projectService: ProjectService,
  private _taskService: TaskService, private _projectRoleService: ProjectRoleService, 
  @Inject('signalRURL') private _signalRURL: string, private _tokenGuardService: TokenGuardService) { 
    this.statusesWithDescription = this._taskService.getStatusesWithDescriptions(false);
    this.tasksConnectionContainer.connection = new signalR.HubConnectionBuilder()
    .withUrl(this._signalRURL + "tasksHub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
    })
    .withAutomaticReconnect()
    .build();
    this.createForm();
  }

  private setLimitsForm(): void {
    const undefinedValue = 'unlimited';
    this.toDoControl.setValue(this.limits.maxToDo ?? undefinedValue);
    this.inProgressControl.setValue(this.limits.maxInProgress ?? undefinedValue);
    this.validateControl.setValue(this.limits.maxValidate ?? undefinedValue);
  }

  private checkRole(role: UserOnProjectRole): void {
    const controls = this.controls;
    if (role < this.userOnProjectRoles.Manager)
      controls.forEach(control => {
        if (!control.disabled)
          control.disable()
      });
    else
      controls.forEach(control => {
        if (!control.enabled)
          control.enable();
      });
  }

  async ngOnInit(): Promise<void> {
    this._titleService.setTitle(`${this.projectName} | Tasks - ${this._websiteName}`);
    this._projectService.getLimits(this.projectId)
    .subscribe({
      next: result => 
      {
        this.limits = result;
        this.setLimitsForm();
      },
      error: error => 
      this._snackBar.open("Max quantities have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
    });
    this.checkRole(this.me.role);
    this._projectService.getTasks(this.projectId)
    .subscribe({
      next: result => 
      {
        this.tasks = result;
        for (let key in result)
          this.tasksCount[key as keyof TasksCountModel] = this.tasks[key as keyof TasksModel].length;
      },
      error: error => 
      {
        this.loadError = true;
        this._snackBar.open("Tasks have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
    this._projectService.getUsersOnPage(this.projectId)
    .subscribe({
      next: result => this.usersOnPage = result,
      error: error => 
      {
        this.loadError = true;
        this._snackBar.open("Users that are currently on page have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
    this.connectionContainer.connection.on("UpdatedLimits", (id: number, model: ProjectLimitsModel) => {
      if (id == this.projectId)
      {
        this.limits = model;
        this.setLimitsForm();
      }
    });
    this.connectionContainer.connection.on("UpdatedUser", (model: UserOnProjectModel) => {
      if (model.userId == this.me.userId && model.projectId == this.projectId)
        this.checkRole(this._projectRoleService.roleToEnum(model.role));
    });
    this.connectionContainer.connection.on("Login", (id: number, model: UserMiniWithAvatarModel) => {
      if (id == this.projectId && this.usersOnPage && this.usersOnPage.findIndex(u => u.id == model.id) == -1)
        this.usersOnPage.push(model);
    });
    this.connectionContainer.connection.on("Logout", (id: number, userId: number) => {
      if (id == this.projectId && this.usersOnPage)
      {
        const foundIndex = this.usersOnPage.findIndex(u => u.id == userId);
        if (foundIndex != -1)
          this.usersOnPage.splice(foundIndex, 1);
      }
    });
    this.connectionContainer.connection.on("AddedTask", (id: number, model: TaskModel) => {
      if (id == this.projectId)
        this.addTask({
          filesCount: 0,
          messagesCount: 0,
          ...model
        }, model.status);
    });
    this.connectionContainer.connection.on("TaskStatusChanged", (id: number, model: TaskStatusChangeModel) => {
      if (id == this.projectId)
      {
        if (model.old != TaskStatus.Archived)
          this.onTaskStatusUpdate(model);
        else
          this._taskService.getReducedById(model.id)
          .subscribe({
            next: result => this.onAddFromArchive({
              status: model.new,
              ...result
            }),
            error: error => console.error(error)
          });
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
  
  public changeLimit(data: any): string
  {
    return data.toString().replace(/\s+$/g, '');
  }

  createForm() {
    this.toDoControl = new FormControl({value: 'unlimited', disabled: false}, [Validators.required, createNumberOrUnlimitedValidator()]);
    this.inProgressControl = new FormControl({value: 'unlimited', disabled: false}, [Validators.required, createNumberOrUnlimitedValidator()]);
    this.validateControl = new FormControl({value: 'unlimited', disabled: false}, [Validators.required, createNumberOrUnlimitedValidator()]);
    this.form = this._fb.group({
      toDo: this.toDoControl,
      inProgress: this.inProgressControl,
      validate: this.validateControl
    });
    const controls = this.controls;
    controls.forEach(control => {
      control.valueChanges
      .subscribe(data => control.setValue(this.changeLimit(data), {emitEvent: false}));
    });

    this.form.valueChanges
    .subscribe(data => this.onValueChanged(data));

    this.onValueChanged();
  }

  onValueChanged(data?: any) {
    if (!this.form)
      return;
    const form = this.form;
    for (const field in this.formErrors)
    {
      if (this.formErrors.hasOwnProperty(field)) {
        this.formErrors[field] = '';
        const control = form.get(field);
        if (control && control.dirty && !control.valid) {
          const messages = this.validationMessages[field];
          for (const key in control.errors) {
            if (control.errors.hasOwnProperty(key)) {
              this.formErrors[field] += messages[key] + ' ';
            }
          }
        }
      }
    }
  }

  onSubmit(event: any) {
    let callerName:string = event.target.attributes.getNamedItem('ng-reflect-name').value;
    callerName = 'max' + callerName.charAt(0).toUpperCase() + callerName.slice(1);
    type ObjectKey = keyof typeof this.limits;
    const callerNameAsKey = callerName as ObjectKey;
    const value = event.target.value == "unlimited" ? undefined : event.target.value;
    if (this.limits[callerNameAsKey] != value && this.form.valid)
    {
      const toDo = parseInt(this.form.get('toDo')?.value);
      const inProgress = parseInt(this.form.get('inProgress')?.value);
      const validate = parseInt(this.form.get('validate')?.value);
      const newLimits:ProjectLimitsModel = 
      {
        maxToDo: isNaN(toDo) ? undefined : toDo,
        maxInProgress: isNaN(inProgress) ? undefined : inProgress,
        maxValidate: isNaN(validate) ? undefined : validate
      };
      this._projectService.updateLimits(this.connectionContainer.id, this.projectId, newLimits)
      .subscribe({
        next: () => {
          this.limits = newLimits;
          this._snackBar.open("Max quantities were updated successfully", "Close", {
            duration: 1000,
            panelClass: "snackbar-orange"
          });
        },
        error: error => { 
          this._snackBar.open(`Max quantities were not updated: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`, "Close", {
            duration: 5000,
          });
        }
      });
    }
  }

  convertStatusToString(status: TaskStatus): string {
    let modifiedStatus: string
    if (status == TaskStatus.Complete)
      modifiedStatus = 'done';
    else
      modifiedStatus = status.charAt(0).toLowerCase() + status.slice(1);
    return modifiedStatus;
  }

  getTasksColumnByStatus(status: string): TaskReducedModel[] {
    type ObjectKey = keyof typeof this.tasks;
    return this.tasks[status as ObjectKey]
  }

  changeCountByStatus(status: string, toAdd: number) {
    type ObjectKey = keyof TasksCountModel;
    this.tasksCount[status as ObjectKey] += toAdd;
  }

  addTask(task: TaskReducedModel, where: TaskStatus) {
    const convertedWhere: string = this.convertStatusToString(where);
    this.getTasksColumnByStatus(convertedWhere).push(task);
    this.changeCountByStatus(convertedWhere, 1);
  }

  onAddedWithTagError(where: TaskStatus) {
    this.changeCountByStatus(this.convertStatusToString(where), 1);
  }

  onTaskStatusUpdate(event: TaskStatusChangeModel): void {
    if (event.old == TaskStatus.Archived)
      return;
    const convertedOld: string = this.convertStatusToString(event.old);
    const oldTasks = this.getTasksColumnByStatus(convertedOld);
    const found = oldTasks?.find(t => t.id == event.id);
    if (found)
    {
      const oldIndex = oldTasks.indexOf(found);
      if (oldIndex != -1)
      {
        this.changeCountByStatus(convertedOld, -1);
        oldTasks.splice(oldIndex, 1);
      }
      if (event.new != TaskStatus.Archived)
        this.addExistingTask(found, event.new);
      this.subscribeToTask(found.id);
    }
  }

  private addExistingTask(task: TaskReducedModel, where: TaskStatus): void {
    const convertedWhere: string = this.convertStatusToString(where);
    const newTasks = this.getTasksColumnByStatus(convertedWhere);
    this.changeCountByStatus(convertedWhere, 1);
    newTasks.splice(this._taskService.getInsertAtIndexByTaskId(task.id, newTasks), 0, task);
  }

  private subscribeToTask(taskId: number): void {
    const foundViewTask = this.viewTasks.find(t => t.model.id == taskId);
    foundViewTask?.updatedStatus.subscribe(m => this.onTaskStatusUpdate(m));
    foundViewTask?.movedFromArchived.subscribe(m => this.onAddFromArchive(m));
  }

  onAddFromArchive(task: TaskReducedWithStatusModel): void
  {
    this.addExistingTask(task, task.status);
    this.subscribeToTask(task.id);
  }

  ngOnDestroy()
  {
    if (this.tasksConnectionContainer.connection && this.tasksConnectionContainer.connection.state == signalR.HubConnectionState.Connected)
      this.tasksConnectionContainer.connection.stop().then(() => this.tasksConnectionContainer = null!);
    else
      this.tasksConnectionContainer = null!
  }
}
