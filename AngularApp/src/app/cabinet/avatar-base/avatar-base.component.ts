import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { AvatarPageMode } from 'src/app/shared/avatar-page-mode';

@Component({
  selector: 'app-avatar-base',
  templateUrl: './avatar-base.component.html',
  styleUrls: ['./avatar-base.component.scss']
})
export class AvatarBaseComponent implements OnInit {
  public mode: AvatarPageMode = AvatarPageMode.Show;
  public modes = AvatarPageMode;

  constructor(public _dialogRef: MatDialogRef<AvatarBaseComponent>) { }

  ngOnInit(): void {
  }

  close() : void {
    this._dialogRef.close();
  }

  onModeChange(mode: AvatarPageMode): void
  {
    this.mode = mode;
  }
}
