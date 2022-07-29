import { Component, Inject, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';

@Component({
  selector: 'app-project-gantt',
  templateUrl: './project-gantt.component.html',
  styleUrls: ['./project-gantt.component.scss']
})
export class ProjectGanttComponent implements OnInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;

  connectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Gantt chart - ${this._websiteName}`);
  }

}
