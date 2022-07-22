import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MessageService } from 'src/app/services/message.service';
import { TokenService } from 'src/app/services/token.service';

@Component({
  selector: 'app-task-message-delete',
  templateUrl: './task-message-delete.component.html',
  styleUrls: ['./task-message-delete.component.scss']
})
export class TaskMessageDeleteComponent implements OnInit {
  private readonly _messageId: number;
  success: boolean = false;
  loading: boolean = false;
  errorMessage: string | null | undefined;
  @Output() succeeded = new EventEmitter();

  constructor(private _dialogRef: MatDialogRef<TaskMessageDeleteComponent>, @Inject(MAT_DIALOG_DATA) public data: number,
  private _tokenService: TokenService, private _messageService: MessageService) {
    this._messageId = data;
  }

  ngOnInit(): void {
  }

  onSubmit(): void {
    this.loading = true;
    this._messageService.delete(this._tokenService.getJwtToken()!, this._messageId).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
          this.succeeded.emit();
        },
        error: error => {
          this.errorMessage = error.error ?? error.message ?? error;
          this.loading = false;
        }
      }
    );
  }
}
