import { Component, Inject, OnInit, QueryList, ViewChildren } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { ProjectService } from 'src/app/services/project.service';
import { TaskService } from 'src/app/services/task.service';
import { ProjectLimitsModel } from 'src/app/shared/project/limits/project-limits.model';
import { TasksCountModel } from 'src/app/shared/project/tasks/tasks-count.model';
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
  limits: ProjectLimitsModel = {
    maxToDo: undefined!,
    maxInProgress: undefined!,
    maxValidate: undefined!
  };
  tasksCount: TasksCountModel = {
    toDo: undefined!,
    inProgress: undefined!,
    validate: undefined!,
    done: undefined!
  }

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string,
  private _projectService: ProjectService, private _snackBar: MatSnackBar,
  private _taskService: TaskService) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Archive - ${this._websiteName}`);
    this._projectService.getArchivedTasks(this.projectId)
    .subscribe({
      next: result => {
        this.tasks = result;
      },
      error: error => {
        this.loadError = true;
        this._snackBar.open("Tasks have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
    this._projectService.getLimits(this.projectId)
    .subscribe({
      next: result => 
      {
        for (let key in result)
        {
          const objectKey = key as keyof typeof result;
          this.limits[objectKey] = result[objectKey];
        }
      },
      error: error => 
      this._snackBar.open("Max quantities have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
    });
    this._projectService.getTasks(this.projectId)
    .subscribe({
      next: result => 
      {
        for (let key in result)
          this.tasksCount[key as keyof TasksCountModel] = result[key as keyof typeof result].length;
      },
      error: error => 
        this._snackBar.open("Tasks have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
    });
  }

  onMovedToArchive(event: TaskReducedModel): void {
    this.tasks.splice(this._taskService.getInsertAtIndexByTaskId(event.id,  this.tasks), 0, event);
    this.subscribeToTask(event.id);
  }

  onMovedFromArchive(event: TaskReducedWithStatusModel): void {
    this.onDeletedTask(event.id);
    this.subscribeToTask(event.id);
  }

  onDeletedTask(id: number): void {
    const index = this.tasks.findIndex(t => t.id == id);
    if (index != -1)
      this.tasks.splice(index, 1);
  }

  private subscribeToTask(taskId: number) {
    const foundViewTask = this.viewTasks.find(t => t.model.id == taskId);
    foundViewTask?.movedToArchived.subscribe(m => this.onMovedToArchive(m));
    foundViewTask?.movedFromArchived.subscribe(m => this.onMovedFromArchive(m));
  }
}
