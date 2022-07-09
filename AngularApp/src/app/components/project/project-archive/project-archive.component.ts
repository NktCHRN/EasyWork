import { Component, Inject, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { UserOnProjectRole } from 'src/app/shared/project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';

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

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Archive - ${this._websiteName}`);
  }

}
