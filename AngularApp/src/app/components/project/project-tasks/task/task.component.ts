import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { Component, Inject, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { TaskService } from 'src/app/services/task.service';
import { TaskPriority } from 'src/app/shared/task/priority/task-priority';
import { TaskPriorityNone } from 'src/app/shared/task/priority/task-priority-none';
import { SavedIconState } from 'src/app/shared/task/save/saved-icon-state';
import { TaskStatusWithDescriptionModel } from 'src/app/shared/task/status/task-status-with-description.model';
import { TaskDialogSettingsModel } from 'src/app/shared/task/task-dialog-settings.model';
import { TaskModel } from 'src/app/shared/task/task.model';
import { UpdateTaskModel } from 'src/app/shared/task/update-task.model';
import { EventEmitter } from '@angular/core';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskDeleteComponent } from '../task-delete/task-delete.component';
import { TasksCountModel } from 'src/app/shared/project/tasks/tasks-count.model';
import { ProjectLimitsModel } from 'src/app/shared/project/limits/project-limits.model';
import { ProjectService } from 'src/app/services/project.service';
import { UserMiniWithAvatarModel } from 'src/app/shared/user/user-mini-with-avatar.model';
import { TaskFilesComponent } from './task-files/task-files.component';
import { ErrorDialogComponent } from 'src/app/components/error-dialog/error-dialog.component';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { TaskStatusChangeModel } from 'src/app/shared/task/status/task-status-change.model';

@Component({
  selector: 'app-task',
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.scss']
})
export class TaskComponent implements OnInit {
  private _taskId: number;
  task: TaskModel = undefined!;
  iconStates = SavedIconState;
  savedIconState: SavedIconState | null = null;
  @ViewChild('nameAutosize') nameAutosize: CdkTextareaAutosize = undefined!;
  @ViewChild('descriptionAutosize') descriptionAutosize: CdkTextareaAutosize = undefined!;
  editName: boolean = false;
  editDescription: boolean = false;
  savedIconColors = {
    [SavedIconState.Fail] : "#D84315",
    [SavedIconState.Loading] : "gray",
    [SavedIconState.Success] : "seagreen"
  };
  form: FormGroup = null!;
  @ViewChild('tform') formDirective: any;
  showToProject: boolean;
  readonly statusesWithDescription: TaskStatusWithDescriptionModel[];
  readonly priorities: (TaskPriority | TaskPriorityNone)[];
  @Output() updatedTask: EventEmitter<UpdateTaskModel> = new EventEmitter<UpdateTaskModel>();
  @Output() deletedTask: EventEmitter<number> = new EventEmitter<number>();
  errorMessage: string | null | undefined;
  readonly taskStatuses = TaskStatus;
  @Output() addedMessage = new EventEmitter();
  @Output() deletedMessage = new EventEmitter();

  @Output() deletedFile = new EventEmitter();
  @Output() addedFile = new EventEmitter();

  limits: ProjectLimitsModel = undefined!;      // subscribe to project's limits change!!!
  tasksCount: TasksCountModel = undefined!;
  @Output() deletedExecutor: EventEmitter<number> = new EventEmitter<number>();
  @Output() addedExecutor: EventEmitter<UserMiniWithAvatarModel> = new EventEmitter<UserMiniWithAvatarModel>();

  @ViewChild(TaskFilesComponent) filesComponent: TaskFilesComponent = undefined!;

  formErrors : any = {
    'name': '',
    'deadline': ''
  };

  validationMessages : any = {
    'name': {
      'required':      'Name is required.',
      'notWhitespace':      'Name cannot be whitespace-only.'
    }
  };

  projectsConnectionContainer: ConnectionContainer = new ConnectionContainer();
  connectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _dialogRef: MatDialogRef<TaskComponent>, @Inject(MAT_DIALOG_DATA) public data: TaskDialogSettingsModel, 
  private _taskService: TaskService, private _snackBar: MatSnackBar, 
  private _fb: FormBuilder, private _router: Router, private _dialog: MatDialog, private _projectService: ProjectService) { 
    this._taskId = data.taskId;
    this.showToProject = data.showToProjectButton;
    this.limits = data.limits;
    this.tasksCount = data.tasksCount;
    this.projectsConnectionContainer = data.projectsConnectionContainer;
    this.connectionContainer = data.connectionContainer;
    this.statusesWithDescription = this._taskService.getStatusesWithDescriptions(true);
    this.priorities = this._taskService.getSortedPriorities();
    this.createForm();
    this._dialogRef.disableClose = true;
    this._dialogRef.backdropClick().subscribe(() => {
      this.close();
    });
  }

  exceedsLimits(status: TaskStatus): boolean {
    if (status == TaskStatus.Archived || status == TaskStatus.Complete || status == this.task.status)
      return false;
    let modifiedStatus: string = status.charAt(0).toLowerCase() + status.substring(1);
    const count: number = this.tasksCount[modifiedStatus as keyof typeof this.tasksCount];
    modifiedStatus = "max" + status;
    const limit: number | null | undefined = this.limits[modifiedStatus as keyof typeof this.limits];
    return limit != null && limit != undefined && limit <= count;
  }

  toProject(): void {
    if(this?.task?.projectId)
    {
      this._dialogRef.close();
      this._router.navigate(['/projects', this.task.projectId]);
    }
  }

  toggleEditName(): void {
    this.editName = !this.editName;
  }

  toggleEditDescription(): void {
    this.editDescription = !this.editDescription;
  }

  createForm() {
    this.form = this._fb.group({
      name: ['', [Validators.required, createNotWhitespaceValidator()] ],
      description: '',
      deadline: '',
      endDate: '',
      status: undefined,
      priority: null
    });

    this.form.valueChanges
    .subscribe(data => this.onValueChanged(data));

    this.onValueChanged();
  }

  ngOnInit(): void {
    this._taskService.getById(this._taskId)
    .subscribe({
      next: result => 
      {
        this.task = result;
        this.form.controls['name'].setValue(this.task.name);
        this.form.controls['description'].setValue(this.task.description);
        this.form.controls['deadline'].setValue(this.task.deadline);
        this.form.controls['endDate'].setValue(this.task.endDate);
        this.form.controls['status'].setValue(this.task.status);
        this.form.controls['priority'].setValue(this._taskService.PriorityOrNullToExtendedPriority(this.task.priority));
        this.checkLimitsAndTasks();
      },
      error: error => {
        const messageStart: string = "Task has not been loaded. Error: ";
        this._snackBar.open(messageStart + JSON.stringify(error), "Close", {duration: 5000});
        this.errorMessage = `${messageStart}${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
      }
    });
    this.projectsConnectionContainer.connection.on("DeletedTask", (_, taskId: number) => {
      if (taskId == this._taskId)
      {
        this._dialog.closeAll();
        this._dialogRef.close();
      }
    });
    this.connectionContainer.connection.on("Updated", (taskId: number, model: UpdateTaskModel) => {
      if (taskId == this._taskId)
      {
        this.task = {
          ...this.task,
          ...model
        };
        this.form.controls['deadline'].setValue(this.task.deadline);
        this.form.controls['endDate'].setValue(this.task.endDate);
        this.form.controls['status'].setValue(this.task.status);
        this.form.controls['priority'].setValue(this._taskService.PriorityOrNullToExtendedPriority(this.task.priority));
      }
    });
  }

  private subscribeToLimitsChange(): void {
    this.projectsConnectionContainer.connection.on("UpdatedLimits", (id: number, model: ProjectLimitsModel) => {
      if (this.task && id == this.task.projectId)
        this.limits = model;
    });
  }

  private subscribeToStatusChange(): void {
    this.projectsConnectionContainer.connection.on("TaskStatusChanged", (id: number, model: TaskStatusChangeModel) => {
      if (this.task && id == this.task.projectId)
      {
        this.changeTasksCountByStatus(model.old, -1);
        this.changeTasksCountByStatus(model.new, 1);
      }
    });
  }

  private subscribeToAdd(): void
  {
    this.projectsConnectionContainer.connection.on("AddedTask", (id: number, model: TaskModel) => {
      if (this.task && id == this.task.projectId)
        this.changeTasksCountByStatus(model.status, 1);
    });
  }

  private changeTasksCountByStatus(status: TaskStatus, value: number)
  {
    switch(status)
    {
      case TaskStatus.ToDo:
        this.tasksCount.toDo += value;
        break;
      case TaskStatus.InProgress:
        this.tasksCount.inProgress += value;
        break;
      case TaskStatus.Validate:
        this.tasksCount.validate += value;
        break;
      case TaskStatus.Complete:
        this.tasksCount.done += value;
        break;
    }
  }

  private checkLimitsAndTasks(): void {
    if (!this.limits) {
      this._projectService.getLimits(this.task.projectId)
      .subscribe({
        next: result => 
        {
          this.limits = result;
          this.subscribeToLimitsChange();
        },
        error: error => 
        this._snackBar.open("Max quantities have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
      });
    }
    else
      this.subscribeToLimitsChange();
    if (!this.tasksCount) {
      this._projectService.getTasks(this.task.projectId)
      .subscribe({
        next: result => 
        {
          this.tasksCount = {
            toDo: undefined!,
            inProgress: undefined!,
            validate: undefined!,
            done: undefined!
          };
          for (let key in result)
          {
            const objectKey = key as keyof typeof result;
            this.tasksCount[objectKey] = result[objectKey].length;
          };
          this.subscribeToStatusChange();
          this.subscribeToAdd();
        },
        error: error => 
          this._snackBar.open("Tasks have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
      });
    }
    else
    {
      this.tasksCount = {...this.tasksCount};
      this.subscribeToStatusChange();
      this.subscribeToAdd();
    }
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
          if (messages)
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
    const callerName:string = event.target.attributes.getNamedItem('ng-reflect-name').value;
    type ObjectKey = keyof typeof this.task;
    const callerNameAsKey = callerName as ObjectKey;
    if ((this.task[callerNameAsKey]) != event.target.value)
      this.send();
  }

  send(): void {
    if (this.form.valid)
    {
      this.switchToLoadingMode();
      const oldStatus = this.task.status;
      this.task = {
        ...this.task,
        ...this.form.value,
        priority: this._taskService.ExtendedPriorityToPriorityOrNull(this.form.get('priority')?.value)
      };
      const updateModel: UpdateTaskModel = {
        ...this.task
      };
      if (oldStatus != this.task.status)
      {
        this.changeTasksCountByStatus(oldStatus, -1);
        this.changeTasksCountByStatus(this.task.status, 1);
      };
      this._taskService.update({
        projectsId: this.projectsConnectionContainer.id,
        tasksId: this.connectionContainer.id
      }, this.task.id, updateModel)
      .subscribe({
        next: () => {
          this.updatedTask.emit(updateModel);
          this.switchToSuccessMode();
          this.editName = this.editDescription = false;
        },
        error: error => {
          this.switchToFailMode();
          console.log(error);
        }
      });
    }
  }

  openDeleteDialog() {
    const dialogRef = this._dialog.open(TaskDeleteComponent, {
      panelClass: "dialog-responsive",
      data: {
        task: {
          ...this.task
        },
        projectsConnection: this.projectsConnectionContainer
      }
    });
    dialogRef.componentInstance.succeeded
    .subscribe(() => {
        this._dialogRef.close();
        this.deletedTask.emit(this.task.id);
    });
  }

  switchToLoadingMode() {
    this.savedIconState = this.iconStates.Loading;
  }

  switchToSuccessMode() {
    this.savedIconState = this.iconStates.Success;
    setTimeout(() => this.savedIconState = null, 1000);
  }

  switchToFailMode() {
    this.savedIconState = this.iconStates.Fail;
  }

  close() {
    if (this.filesComponent && 
      ((this.filesComponent.files && this.filesComponent.files.filter(f => f.loadingParameters).length > 0) || this.filesComponent.uploading))
      {
        this._dialogRef.disableClose = true;
        this._dialog.open(ErrorDialogComponent, {
          panelClass: "dialog-responsive",
          data: "You have some unfinished uploads. Please, either wait for them to finish or cancel them"
        });
      }
      else {
        this._dialogRef.close();
      }
  }
}
