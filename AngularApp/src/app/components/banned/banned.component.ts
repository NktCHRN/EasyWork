import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BannedModel } from '../../shared/user/banned.model';

@Component({
  selector: 'app-banned',
  templateUrl: './banned.component.html',
  styleUrls: ['./banned.component.scss']
})
export class BannedComponent implements OnInit {
  bans: BannedModel[] = [];

  constructor(private _dialogRef: MatDialogRef<BannedComponent>, @Inject(MAT_DIALOG_DATA) public data: BannedModel[]) { 
    this.bans = data;
  }

  ngOnInit(): void {
  }

}
