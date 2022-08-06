import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BanService } from 'src/app/services/ban.service';
import { BanDeletePageModel } from 'src/app/shared/ban/ban-delete-page.model';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';

@Component({
  selector: 'app-ban-delete',
  templateUrl: './ban-delete.component.html',
  styleUrls: ['./ban-delete.component.scss']
})
export class BanDeleteComponent implements OnInit {
  @Output() succeeded = new EventEmitter();
  private readonly _id: number;
  private readonly _userId: number;
  errorMessage: string | null | undefined;
  loading: boolean = false;
  success: boolean = false;

  connectionContainer: ConnectionContainer;

  constructor(private _dialogRef: MatDialogRef<BanDeleteComponent>, @Inject(MAT_DIALOG_DATA) public data: BanDeletePageModel,
  private _banService: BanService) {
    this._id = data.id;
    this._userId = data.userId;
    this.connectionContainer = data.connectionContainer;
  }

  ngOnInit(): void {
    this.connectionContainer.connection.on("DeletedBan", (_, banId: number) =>
    {
      if (banId == this._id)
        this._dialogRef.close();
    });
    this.connectionContainer.connection.on("Unbanned", (userId: number) =>
    {
      if (userId == this._userId)
        this._dialogRef.close();
    });
  }

  public onSubmit(): void
  {
    this.loading = true;
    this._banService.delete(this.connectionContainer.id, this._id)
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
