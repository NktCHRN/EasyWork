import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { TaskExtraReducedModel } from 'src/app/shared/task/task-extra-reduced.model';

@Component({
  selector: 'app-task-delete',
  templateUrl: './task-delete.component.html',
  styleUrls: ['./task-delete.component.scss']
})
export class TaskDeleteComponent implements OnInit {
  model: TaskExtraReducedModel;
  success: boolean = false;
  loading: boolean = false;
  errorMessage: string | null | undefined;

  constructor(private _dialogRef: MatDialogRef<TaskDeleteComponent>, @Inject(MAT_DIALOG_DATA) public data: TaskExtraReducedModel, 
  private _taskService: TaskService, private _tokenService: TokenService) {
    this.model = data;
  }

  ngOnInit(): void {
  }

  onSubmit(): void {
    this.loading = true;
    this._taskService.delete(this._tokenService.getJwtToken()!, this.model.id).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
        },
        error: error => {
          this.errorMessage = error.error ?? error.message ?? error;
          this.loading = false;
        }
      }
    );
  }
}
