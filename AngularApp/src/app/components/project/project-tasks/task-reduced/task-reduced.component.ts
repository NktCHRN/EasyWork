import { Component, Input, OnInit } from '@angular/core';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';
import { UserMiniWithAvatarModel } from 'src/app/shared/user/user-mini-with-avatar.model';

@Component({
  selector: 'app-task-reduced',
  templateUrl: './task-reduced.component.html',
  styleUrls: ['./task-reduced.component.scss']
})
export class TaskReducedComponent implements OnInit {
  @Input() model: TaskReducedModel = undefined!;
  executors: UserMiniWithAvatarModel[] = [];

  constructor(private _taskService: TaskService, private _tokenService: TokenService) { }

  ngOnInit(): void {
    this._taskService.getExecutors(this._tokenService.getJwtToken()!, this.model.id)
    .subscribe(result => this.executors = result);
  }

  openDialog()
  {
    console.log("The dialog should have been opened");
  }
}
