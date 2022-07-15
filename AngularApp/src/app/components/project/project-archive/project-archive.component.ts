import { Component, Inject, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';

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

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string,
  private _tokenService: TokenService, private _projectService: ProjectService, private _snackBar: MatSnackBar) { }

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

}
