import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { UserInfoService } from 'src/app/services/userinfo.service';
import { AvatarPageMode } from 'src/app/shared/user/cabinet/avatar-page-mode';
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

  constructor(private _userInfoService: UserInfoService, private _dialog: MatDialog) { }

  ngOnInit(): void {
    this._userInfoService.lastUser.subscribe(user => this.linkToAvatar = user?.avatarURL);
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
