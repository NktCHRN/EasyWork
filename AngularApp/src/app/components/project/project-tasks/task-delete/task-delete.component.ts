import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TaskService } from 'src/app/services/task.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { TaskDeleteDialogSettingsModel } from 'src/app/shared/task/delete/task-delete-dialog-settings.model';
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
  @Output() succeeded = new EventEmitter();

  projectsConnectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _dialogRef: MatDialogRef<TaskDeleteComponent>, @Inject(MAT_DIALOG_DATA) public data: TaskDeleteDialogSettingsModel, 
  private _taskService: TaskService) {
    this.model = data.task;
    this.projectsConnectionContainer = data.projectsConnection;
  }

  ngOnInit(): void {
  }

  onSubmit(): void {
    this.loading = true;
    this._taskService.delete(this.projectsConnectionContainer.id, this.model.id).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
          this.succeeded.emit();
        },
        error: error => {
          this.errorMessage = error.error ?? error.message ?? error;
          this.loading = false;
        }
      }
    );
  }
}
