import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { AccountService } from 'src/app/services/account.service';
import { TokenService } from 'src/app/services/token.service';
import { UserinfoService } from 'src/app/services/userinfo.service';

@Component({
  selector: 'app-avatar-delete',
  templateUrl: './avatar-delete.component.html',
  styleUrls: ['./avatar-delete.component.scss']
})
export class AvatarDeleteComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  @Output() closeOuter = new EventEmitter();

  constructor(private _dialogRef: MatDialogRef<AvatarDeleteComponent>,
    private _accountService: AccountService,
    private _userInfoService: UserinfoService,
    private _tokenService: TokenService) {
      this._dialogRef.afterClosed()
      .subscribe(() => {
        if (this.success)
          this.closeOuter.emit();
      });
  }

  ngOnInit(): void {
  }

  onSubmit() : void {
    this.loading = true;
    this._accountService.deleteAvatar(this._tokenService.getJwtToken()!).subscribe(
      {
        next: () => {
          this.success = true;
          this._userInfoService.updateLastUser();
          this.loading = false;
        },
        error: error => {
          this.errorMessage = error.error ?? error.message ?? error;
          this.loading = false;
        }
      }
    );
  }
}
