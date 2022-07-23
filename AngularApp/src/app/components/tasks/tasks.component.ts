import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TaskService } from '../../services/task.service';
import { UserTaskModel } from '../../shared/task/user-task.model';
import { TaskComponent } from '../project/project-tasks/task/task.component';

@Component({
  selector: 'app-tasks',
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.scss']
})
export class TasksComponent implements OnInit {
  tasks: UserTaskModel[] | null | undefined;
  errorMessage: string | null | undefined;
  loading: boolean = true;
  readonly statuses: Map<string, string>;

  constructor(private _tasksService: TaskService, private _dialog: MatDialog) {
     this.statuses = new Map(
      _tasksService.getStatusesWithDescriptions(true).map(object => {
        return [object.status.toString(), object.description];
      }),
    );
   }

  ngOnInit(): void {
    this._tasksService.get()
    .subscribe({
      next: tasks => 
      {
        this.tasks = tasks; 
        this.loading = false;
      },
      error: error => 
      {
        this.errorMessage = typeof error === 'string' || error instanceof String ? error : error.message; 
        this.loading = false;
      },
    });
  }

  openDialog(id: number): void {
    const dialogRef = this._dialog.open(TaskComponent, {
      panelClass: "dialog-responsive",
      data: {
        taskId: id,
        showToProjectButton: true
      }
    });
    dialogRef.componentInstance.updatedTask.subscribe(
      task => {
        let foundTask = this.tasks?.find(t => t.id == id);
        if (foundTask)
        {
          foundTask.name = task.name;
          foundTask.deadline = task.deadline;
          foundTask.endDate = task.endDate;
          foundTask.status = task.status;
        }
      }
    );
  }
}
