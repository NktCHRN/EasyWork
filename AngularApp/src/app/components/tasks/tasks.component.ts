import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TokenService } from 'src/app/services/token.service';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
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
  private readonly _myId: number;

  constructor(private _tasksService: TaskService, private _dialog: MatDialog, private _tokenService: TokenService) {
     this.statuses = new Map(
      _tasksService.getStatusesWithDescriptions(true).map(object => {
        return [object.status.toString(), object.description];
      }),
    );
    this._myId = this._tokenService.getMyId()!;
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
        const foundTask = this.tasks?.find(t => t.id == id);
        if (foundTask)
        {
          const oldStatus = foundTask?.status;
          foundTask.name = task.name;
          foundTask.deadline = task.deadline;
          foundTask.endDate = task.endDate;
          foundTask.status = task.status;
          if (this._tasksService.isDone(oldStatus as TaskStatus) != this._tasksService.isDone(task.status))
          {
            const index = this.tasks!.indexOf(foundTask);
            if (index != -1)
            {
              this.tasks?.splice(index, 1);
              this._tasksService.insertUserTask(foundTask, this.tasks!);
            }
          }
        }
      }
    );
    dialogRef.componentInstance.deletedTask.subscribe(
      () => {
        let found = this.tasks?.findIndex(t => t.id == id);
        if (found != undefined && found != null && found != -1)
          this.tasks?.splice(found, 1);
      }
    );
    dialogRef.componentInstance.deletedExecutor.subscribe(
      userId => {
        if (userId == this._myId)
        {
          let found = this.tasks?.findIndex(t => t.id == id);
          if (found != undefined && found != null && found != -1)
            this.tasks?.splice(found, 1);
        }
      }
    );
    dialogRef.componentInstance.addedExecutor.subscribe(
      ex => {
        if (ex.id == this._myId && this.tasks)
        {
          this._tasksService.getById(id).subscribe({
            next: result => this._tasksService.insertUserTask(result, this.tasks!),
            error: error => {
              console.error(error);
            }
          });
        }
      }
    );
  }
}
