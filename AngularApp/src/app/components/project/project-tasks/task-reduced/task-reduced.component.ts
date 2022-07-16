import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
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
  executors: UserMiniWithAvatarModel[] = [];
  prioritiesWithColors: any;

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
      data: this.model.id
    });
    // UPDATE ON CHANGE!!! (with emitter(s) or a dialogRef)
  }
}
