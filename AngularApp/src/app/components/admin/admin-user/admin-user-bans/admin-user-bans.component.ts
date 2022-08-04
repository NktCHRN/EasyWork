import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TimeService } from 'src/app/services/time.service';
import { TokenService } from 'src/app/services/token.service';
import { UserService } from 'src/app/services/user.service';
import { BannedModel } from 'src/app/shared/user/banned.model';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';
import { BanAddComponent } from './ban-add/ban-add.component';

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
  }

  private waitForTheEndOfBan(model: BannedModel)
  {
    const banTime = new Date(model.dateTo);
    const subtractionResult = banTime.getTime() - new Date().getTime();
    if (subtractionResult < this._timeService.daysToMilliseconds(1))
    {
      setTimeout(() => 
      {
        const foundIndex = this.bans.findIndex(b => b.id == model.id);
        if (foundIndex != -1)
          this.bans.splice(foundIndex, 1);
      }, subtractionResult + 2000);
    }
  }

  private addBan(model: BannedModel): void
  {
    if (!this.bans)
      return;
    this.bans.unshift(model);
    this.waitForTheEndOfBan(model);
  }

  public openBanDialog(): void
  {
    const dialogRef = this._dialog.open(BanAddComponent, {
      panelClass: "dialog-responsive",
      data: {
        user: this.user
      }
    });
    dialogRef.componentInstance.succeeded
    .subscribe(ban => {
        this.addBan(ban);
    });
  }
}
