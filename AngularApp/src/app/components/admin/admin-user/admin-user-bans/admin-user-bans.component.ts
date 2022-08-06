import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TimeService } from 'src/app/services/time.service';
import { TokenService } from 'src/app/services/token.service';
import { UserService } from 'src/app/services/user.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { BannedModel } from 'src/app/shared/user/banned.model';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';
import { BanAddComponent } from './ban-add/ban-add.component';
import { BanDeleteComponent } from './ban-delete/ban-delete.component';
import { UnbanComponent } from './unban/unban.component';

@Component({
  selector: 'app-admin-user-bans',
  templateUrl: './admin-user-bans.component.html',
  styleUrls: ['./admin-user-bans.component.scss']
})
export class AdminUserBansComponent implements OnInit {
  @Input() user: UserProfileReducedModel = undefined!;
  myId: number = undefined!;
  bans: BannedModel[] = undefined!;
  errorMessage: string | null | undefined;
  @Input() connectionContainer: ConnectionContainer = undefined!;

  constructor(private _tokenService: TokenService, private _userService: UserService, private _timeService: TimeService,
    private _dialog: MatDialog) { }

  ngOnInit(): void {
    this.myId = this._tokenService.getMyId()!;
    this._userService.getActiveBansById(this.user.id)
    .subscribe({
      next: result => 
      {
        this.bans = result;
        this.bans.forEach(b => this.waitForTheEndOfBan(b));
      },
      error: error =>
      {
        const message = `An error occured: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.errorMessage = message;
        console.error(message);
      }
    });
    this.connectionContainer.connection.on("AddedBan", (userId: number, model: BannedModel) =>
    {
      if (userId == this.user.id)
        this.addBan(model);
    });
    this.connectionContainer.connection.on("DeletedBan", (userId: number, banId: number) =>
    {
      if (userId == this.user.id)
        this.deleteBan(banId);
    });
    this.connectionContainer.connection.on("Unbanned", (userId: number) =>
    {
      if (userId == this.user.id)
        this.unban();
    });
  }

  private waitForTheEndOfBan(model: BannedModel)
  {
    const banTime = new Date(model.dateTo);
    const subtractionResult = banTime.getTime() - new Date().getTime();
    if (subtractionResult < this._timeService.daysToMilliseconds(1))
    {
      setTimeout(() => 
      {
        this.deleteBan(model.id);
      }, subtractionResult + 2000);
    }
  }

  private deleteBan(id: number)
  {
    if (!this.bans)
      return;
    const foundIndex = this.bans.findIndex(b => b.id == id);
    if (foundIndex != -1)
      this.bans.splice(foundIndex, 1);
  }

  private addBan(model: BannedModel): void
  {
    if (!this.bans)
      return;
    this.bans.unshift(model);
    this.waitForTheEndOfBan(model);
  }

  private unban(): void
  {
    this.bans = [];
  }

  public openBanDialog(): void
  {
    const dialogRef = this._dialog.open(BanAddComponent, {
      panelClass: "dialog-responsive",
      data: {
        user: this.user,
        connectionContainer: this.connectionContainer
      }
    });
    dialogRef.componentInstance.succeeded
    .subscribe(ban => {
        this.addBan(ban);
    });
  }

  public openDeleteDialog(id: number): void
  {
    const dialogRef = this._dialog.open(BanDeleteComponent, {
      panelClass: "dialog-responsive",
      data: {
        id: id,
        userId: this.user.id,
        connectionContainer: this.connectionContainer
      }
    });
    dialogRef.componentInstance.succeeded
    .subscribe(() => {
        this.deleteBan(id);
    });
  }

  public openUnbanDialog(): void
  {
    const dialogRef = this._dialog.open(UnbanComponent, {
      panelClass: "dialog-responsive",
      data: {
        user: this.user,
        connectionContainer: this.connectionContainer
      }
    });
    dialogRef.componentInstance.succeeded
    .subscribe(() => {
        this.unban();
    });
  }
}
