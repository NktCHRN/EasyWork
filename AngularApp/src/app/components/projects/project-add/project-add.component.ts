import { Component, OnInit } from '@angular/core';
import { MatButtonToggleChange } from '@angular/material/button-toggle';
import { MatDialogRef } from '@angular/material/dialog';
import { AddProjectPageMode } from 'src/app/shared/project/add-project-page-mode';

@Component({
  selector: 'app-project-add',
  templateUrl: './project-add.component.html',
  styleUrls: ['./project-add.component.scss']
})
export class ProjectAddComponent implements OnInit {
  public mode: AddProjectPageMode = AddProjectPageMode.Join;
  public modes = AddProjectPageMode;

  constructor(private _dialogRef: MatDialogRef<ProjectAddComponent>) { }

  ngOnInit(): void {
  }

  onModeChange($event: MatButtonToggleChange): void {
    this.mode = $event.value;
  }

  closeDialog()
  {
    this._dialogRef.close();
  }
}
