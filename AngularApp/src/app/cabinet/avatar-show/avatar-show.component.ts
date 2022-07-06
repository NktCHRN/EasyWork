import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { UserinfoService } from 'src/app/services/userinfo.service';
import { AvatarPageMode } from 'src/app/shared/avatar-page-mode';
import { AvatarDeleteComponent } from '../avatar-delete/avatar-delete.component';

@Component({
  selector: 'app-avatar-show',
  templateUrl: './avatar-show.component.html',
  styleUrls: ['./avatar-show.component.scss']
})
export class AvatarShowComponent implements OnInit {
  public linkToAvatar: string | null | undefined;
  @Output() closeOuter = new EventEmitter();
  @Output() changeMode = new EventEmitter<AvatarPageMode>();

  constructor(private _userInfoService: UserinfoService, private _dialog: MatDialog) { }

  ngOnInit(): void {
    this.linkToAvatar = this._userInfoService.getLastUser()?.avatarURL;
  }

  openDeleteDialog() : void {
    let dialogRef = this._dialog.open(AvatarDeleteComponent, {
      panelClass: "mini-dialog-responsive"
    });
    dialogRef.componentInstance.closeOuter.subscribe(() => {this.closeOuter.emit()});
  }

  changeToEdit(): void {
    this.changeMode.emit(AvatarPageMode.Edit);
  }
}