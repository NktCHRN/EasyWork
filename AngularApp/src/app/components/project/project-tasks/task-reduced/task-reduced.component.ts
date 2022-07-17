import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskStatusChangeModel } from 'src/app/shared/task/status/task-status-change.model';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';
import { UserMiniWithAvatarModel } from 'src/app/shared/user/user-mini-with-avatar.model';
import { TaskComponent } from '../task/task.component';

@Component({
  selector: 'app-task-reduced',
  templateUrl: './task-reduced.component.html',
  styleUrls: ['./task-reduced.component.scss']
})
export class TaskReducedComponent implements OnInit {
  @Input() model: TaskReducedModel = undefined!;
  @Input() status: TaskStatus = undefined!
  executors: UserMiniWithAvatarModel[] = [];
  prioritiesWithColors: any;
  @Output() updatedStatus: EventEmitter<TaskStatusChangeModel> = new EventEmitter<TaskStatusChangeModel>();

  constructor(private _taskService: TaskService, private _tokenService: TokenService, private _dialog: MatDialog) { }

  ngOnInit(): void {
    this.prioritiesWithColors = this._taskService.getPrioritiesWithColors();
    this._taskService.getExecutors(this._tokenService.getJwtToken()!, this.model.id)
    .subscribe(result => this.executors = result);
  }

  openDialog()
  {
    const dialogRef = this._dialog.open(TaskComponent, {
      panelClass: "dialog-responsive",
      data: {
        taskId: this.model.id,
        showToProjectButton: false
      }
    });
    dialogRef.componentInstance.updatedTask.subscribe(
      task => {
        this.model.deadline = task.deadline;
        this.model.endDate = task.endDate;
        this.model.name = task.name;
        this.model.priority = task.priority;
        if (this.status != task.status)
        {
          console.log("emitted");
          this.updatedStatus.emit({
            old: this.status,
            new: task.status,
            id: this.model.id
          });
          this.status = task.status
        }
      }
    );
    // add update for a files/messages quantity change
  }
}
