import { Component, OnInit } from '@angular/core';
import { TasksService } from '../services/tasks.service';
import { TokenService } from '../services/token.service';
import { UserTaskModel } from '../shared/user-task.model';

@Component({
  selector: 'app-tasks',
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.scss']
})
export class TasksComponent implements OnInit {
  tasks: UserTaskModel[] | null | undefined;
  errorMessage: string | null | undefined;
  loading: boolean = true;

  constructor(private _tasksService: TasksService, private _tokenService: TokenService) { }

  ngOnInit(): void {
    this._tasksService.get(this._tokenService.getJwtToken()!)
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

}
