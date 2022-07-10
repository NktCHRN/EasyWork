import { Component, Input, OnInit } from '@angular/core';
import { ProjectModel } from 'src/app/shared/project/project.model';

@Component({
  selector: 'app-project-info-show',
  templateUrl: './project-info-show.component.html',
  styleUrls: ['./project-info-show.component.scss']
})
export class ProjectInfoShowComponent implements OnInit {
  @Input() project: ProjectModel = undefined!;

  constructor() { }

  ngOnInit(): void {
  }

}
