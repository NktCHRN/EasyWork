import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { TaskExecutorDeletePageModel } from 'src/app/shared/task/executor/task-executor-delete-page.model';
import { UserMiniReducedModel } from 'src/app/shared/user/user-mini-reduced.model';

@Component({
  selector: 'app-task-executor-delete',
  templateUrl: './task-executor-delete.component.html',
  styleUrls: ['./task-executor-delete.component.scss']
})
export class TaskExecutorDeleteComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  private _taskId: number;
  user: UserMiniReducedModel;
  @Output() succeeded = new EventEmitter();

  constructor(private _dialogRef: MatDialogRef<TaskExecutorDeleteComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: TaskExecutorDeletePageModel,
    private _taskService: TaskService, private _tokenService: TokenService) {
      this._taskId = data.taskId;
      this.user = data.user;
  }

  ngOnInit(): void {
  }

  onSubmit()
  {
    this.loading = true;
      this._taskService.deleteExecutor(this._tokenService.getJwtToken()!, this._taskId, this.user.id).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
          this.succeeded.emit();
        },
        error: error => {
          this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          this.loading = false;
        }
      }
      );
  }
}
