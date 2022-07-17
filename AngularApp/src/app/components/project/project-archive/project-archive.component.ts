import { Component, Inject, OnInit, QueryList, ViewChildren } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { ProjectService } from 'src/app/services/project.service';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskReducedWithStatusModel } from 'src/app/shared/task/task-reduced-with-status.model';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';
import { TaskReducedComponent } from '../project-tasks/task-reduced/task-reduced.component';

@Component({
  selector: 'app-project-archive',
  templateUrl: './project-archive.component.html',
  styleUrls: ['./project-archive.component.scss']
})
export class ProjectArchiveComponent implements OnInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;
  tasks: TaskReducedModel[] = undefined!;
  loadError: boolean = false;
  @ViewChildren(TaskReducedComponent) viewTasks: QueryList<TaskReducedComponent> = undefined!;
  taskStatuses = TaskStatus;

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string,
  private _tokenService: TokenService, private _projectService: ProjectService, private _snackBar: MatSnackBar,
  private _taskService: TaskService) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Archive - ${this._websiteName}`);
    this._projectService.getArchivedTasks(this._tokenService.getJwtToken()!, this.projectId)
    .subscribe({
      next: result => {
        this.tasks = result;
      },
      error: error => {
        this.loadError = true;
        this._snackBar.open("Tasks have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
  }

  onMovedToArchive(event: TaskReducedModel): void {
    this.tasks.splice(this._taskService.getInsertAtIndexByTaskId(event.id,  this.tasks), 0, event);
    this.subscribeToTask(event.id);
  }

  onMovedFromArchive(event: TaskReducedWithStatusModel): void {
    const index = this.tasks.findIndex(t => t.id == event.id);
    if (index != -1)
      this.tasks.splice(index, 1);
    this.subscribeToTask(event.id);
  }

  private subscribeToTask(taskId: number) {
    const foundViewTask = this.viewTasks.find(t => t.model.id == taskId);
    foundViewTask?.movedToArchived.subscribe(m => this.onMovedToArchive(m));
    foundViewTask?.movedFromArchived.subscribe(m => this.onMovedFromArchive(m));
  }
}
