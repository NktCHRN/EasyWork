import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { Component, Inject, NgZone, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
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

  constructor(private _dialogRef: MatDialogRef<TaskComponent>, @Inject(MAT_DIALOG_DATA) public data: TaskDialogSettingsModel, 
  private _taskService: TaskService, private _tokenService: TokenService, private _snackBar: MatSnackBar, 
  private _fb: FormBuilder, private _router: Router, private _dialog: MatDialog) { 
    this._taskId = data.taskId;
    this.showToProject = data.showToProjectButton;
    this.statusesWithDescription = this._taskService.getStatusesWithDescriptions(true);
    this.priorities = this._taskService.getSortedPriorities();
    this.createForm();
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
    this._taskService.getById(this._tokenService.getJwtToken()!, this._taskId)
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
      },
      error: error => {
        const messageStart: string = "Task has not been loaded. Error: ";
        this._snackBar.open(messageStart + JSON.stringify(error), "Close", {duration: 5000});
        this.errorMessage = `${messageStart}${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
      }
    });
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
      this.task = {
        ...this.task,
        ...this.form.value,
        priority: this._taskService.ExtendedPriorityToPriorityOrNull(this.form.get('priority')?.value)
      };
      const updateModel: UpdateTaskModel = {
        ...this.task
      };
      this._taskService.update(this._tokenService.getJwtToken()!, this.task.id, updateModel)
      .subscribe({
        next: () => {
          this.updatedTask.emit(updateModel);
          this.switchToSuccessMode();
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
        ...this.task
      }
    });
    dialogRef.afterClosed()
    .subscribe(() => {
      if (dialogRef.componentInstance.success)
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
}
