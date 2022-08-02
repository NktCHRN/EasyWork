import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MessageService } from 'src/app/services/message.service';
import { DeleteMessagePageModel } from 'src/app/shared/message/delete-message-page.model';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';

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

  tasksConnectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _dialogRef: MatDialogRef<TaskMessageDeleteComponent>, @Inject(MAT_DIALOG_DATA) public data: DeleteMessagePageModel,
  private _messageService: MessageService) {
    this._messageId = data.id;
    this.tasksConnectionContainer = data.tasksConnectionContainer
  }

  ngOnInit(): void {
    this.tasksConnectionContainer.connection.on("DeletedMessage", (_, messageId: number) =>
    {
      if (this._messageId == messageId)
        this._dialogRef.close();
    });
  }

  onSubmit(): void {
    this.loading = true;
    this._messageService.delete(this.tasksConnectionContainer.id, this._messageId).subscribe(
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
