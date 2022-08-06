import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UserService } from 'src/app/services/user.service';
import { UnbanPageModel } from 'src/app/shared/ban/unban-page.model';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';

@Component({
  selector: 'app-unban',
  templateUrl: './unban.component.html',
  styleUrls: ['./unban.component.scss']
})
export class UnbanComponent implements OnInit {
  @Output() succeeded = new EventEmitter();
  user: UserProfileReducedModel;
  success: boolean = false;
  loading: boolean = false;
  errorMessage: string | null | undefined;

  connectionContainer: ConnectionContainer;

  constructor(private _dialogRef: MatDialogRef<UnbanComponent>, @Inject(MAT_DIALOG_DATA) public data: UnbanPageModel,
  private _userService: UserService) {
    this.user = data.user;
    this.connectionContainer = data.connectionContainer;
  }

  ngOnInit(): void {
    this.connectionContainer.connection.on("Unbanned", (userId: number) =>
    {
      if (userId == this.user.id)
        this._dialogRef.close();
    });
  }

  onSubmit(): void
  {
    this.loading = true;
    this._userService.unbanById(this.connectionContainer.id, this.user.id)
    .subscribe({
      next: () =>
      {
        this.loading = false;
        this.success = true;
        this.succeeded.emit();
      },
      error: error =>
      {
        this.loading = false;
        const message = `An error occured: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.errorMessage = message;
        console.error(message);
      }
    });
  }
}
