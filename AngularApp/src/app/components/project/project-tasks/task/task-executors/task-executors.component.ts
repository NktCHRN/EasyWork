import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ProjectService } from 'src/app/services/project.service';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { AddExecutorModel } from 'src/app/shared/task/executor/add-executor.model';
import { UserMiniWithAvatarModel } from 'src/app/shared/user/user-mini-with-avatar.model';
import { TaskExecutorDeleteComponent } from './task-executor-delete/task-executor-delete.component';

@Component({
  selector: 'app-task-executors',
  templateUrl: './task-executors.component.html',
  styleUrls: ['./task-executors.component.scss']
})
export class TaskExecutorsComponent implements OnInit {
  @Input() projectId: number = undefined!;
  @Input() taskId: number = undefined!;
  @Output() closeDialog = new EventEmitter();
  @Output() switchedToLoading = new EventEmitter();
  @Output() switchedToSuccess = new EventEmitter();
  @Output() switchedToFail = new EventEmitter();
  executors: UserMiniWithAvatarModel[] = undefined!;
  usersOnProject: UserMiniWithAvatarModel[] = undefined!;
  freeUsersOnProject: UserMiniWithAvatarModel[] = undefined!;
  errorMessage: string | null | undefined;
  editable: boolean = false;

  @Output() deletedExecutor: EventEmitter<number> = new EventEmitter<number>();
  @Output() addedExecutor: EventEmitter<UserMiniWithAvatarModel> = new EventEmitter<UserMiniWithAvatarModel>();

  showForm: boolean = false;
  form: FormGroup = null!;
  @ViewChild('eform') formDirective: any;
  formErrors : any = {
    'id': ''
  };
  validationMessages : any = {
    'id': {
      'required':      'Required'
    }
  };

  constructor(private _taskService: TaskService, private _tokenService: TokenService, private _dialog: MatDialog, 
    private _projectService: ProjectService, private _fb: FormBuilder) {
      this.createForm();
  }

  createForm() {
    this.form = this._fb.group({
      id: ['', [Validators.required] ]
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

  onSubmit(): void {
    if (this.form.valid)
    {
      this.switchedToLoading.emit();
      const model: AddExecutorModel = this.form.value;
      this._taskService.addExecutor(this._tokenService.getJwtToken()!, this.taskId, model)
      .subscribe({
        next: result => {
          this.switchedToSuccess.emit();
          this.addedExecutor.emit(result);
          this.executors.push(result);
          const found = this.freeUsersOnProject.findIndex(u => u.id == result.id);
          if (found != -1)
            this.freeUsersOnProject.splice(found, 1);
          this.turnOffShowForm();
        },
        error: error => {
          this.switchedToFail.emit();
          console.log(error);
        }
      });
    }
  }

  ngOnInit(): void {
    this._taskService.getExecutors(this._tokenService.getJwtToken()!, this.taskId)
    .subscribe({
      next: result => 
      {
        this.executors = result;
        this._projectService.getUsersWithoutRoles(this._tokenService.getJwtToken()!, this.projectId)
    .subscribe({
      next: result => 
      {
        this.usersOnProject = result;
        this.freeUsersOnProject = this.usersOnProject.filter(u => this.executors.findIndex(e => e.id == u.id) == -1);
      },
      error: error => 
      {
        this.errorMessage = `$Executors have not been loaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
      }
    });
      },
      error: error => {
        this.errorMessage = `$Executors have not been loaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
      }
    });
  }

  toggleEditable(): void {
    this.editable = !this.editable;
  }

  turnOnShowForm(): void {
    this.showForm = true;
  }

  turnOffShowForm(): void {
    this.form.reset();
    this.showForm = false;
  }

  openDeleteDialog(id: number): void {
    const user = this.executors.find(u => u.id == id);
    if (user){
      const dialogRef = this._dialog.open(TaskExecutorDeleteComponent, {
        panelClass: "dialog-responsive",
        data: {
          taskId: this.taskId,
          user: user
        }
      });
      dialogRef.afterClosed()
      .subscribe(() => {
        if (dialogRef.componentInstance.success)
        {
          this.executors.splice(this.executors.indexOf(user!), 1);
          this.deletedExecutor.emit(user.id);
          const found = this.usersOnProject.find(u => u.id == user.id);
          if (found)
            this.freeUsersOnProject.push(found);
        };
      });
    }
  }
}
