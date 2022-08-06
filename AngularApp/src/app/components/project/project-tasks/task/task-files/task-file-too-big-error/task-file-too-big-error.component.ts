import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TooBigErrorPageModel } from 'src/app/shared/file/file-too-big-error-page.model';

@Component({
  selector: 'app-task-file-too-big-error',
  templateUrl: './task-file-too-big-error.component.html',
  styleUrls: ['./task-file-too-big-error.component.scss']
})
export class TaskFileTooBigErrorComponent implements OnInit {

  constructor(private _dialogRef: MatDialogRef<TaskFileTooBigErrorComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: TooBigErrorPageModel) { }

  ngOnInit(): void {
  }

}
