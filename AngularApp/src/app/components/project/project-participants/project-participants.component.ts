import { Component, Inject, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';

@Component({
  selector: 'app-project-participants',
  templateUrl: './project-participants.component.html',
  styleUrls: ['./project-participants.component.scss']
})
export class ProjectParticipantsComponent implements OnInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Participants - ${this._websiteName}`);
  }

}
