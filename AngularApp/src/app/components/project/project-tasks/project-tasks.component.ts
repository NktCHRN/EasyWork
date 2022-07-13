import { Component, Inject, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';

@Component({
  selector: 'app-project-tasks',
  templateUrl: './project-tasks.component.html',
  styleUrls: ['./project-tasks.component.scss']
})
export class ProjectTasksComponent implements OnInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Tasks - ${this._websiteName}`);
  }

}
