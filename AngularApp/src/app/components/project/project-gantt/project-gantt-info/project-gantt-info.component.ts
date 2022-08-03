import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-project-gantt-info',
  templateUrl: './project-gantt-info.component.html',
  styleUrls: ['./project-gantt-info.component.scss']
})
export class ProjectGanttInfoComponent implements OnInit {

  constructor(private _dialogRef: MatDialogRef<ProjectGanttInfoComponent>) { }

  ngOnInit(): void {
  }

}
