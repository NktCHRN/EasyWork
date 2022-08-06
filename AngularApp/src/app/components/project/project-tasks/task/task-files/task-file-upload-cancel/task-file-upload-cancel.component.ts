import { Component, Inject, Input, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FileService } from 'src/app/services/file.service';
import { FileModel } from 'src/app/shared/file/file.model';
import { EventEmitter } from '@angular/core';
import { DeleteFilePageModel } from 'src/app/shared/file/delete-file-page.model';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';

@Component({
  selector: 'app-task-file-upload-cancel',
  templateUrl: './task-file-upload-cancel.component.html',
  styleUrls: ['./task-file-upload-cancel.component.scss']
})
export class TaskFileUploadCancelComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  cancellationStarted = new EventEmitter();
  cancelled = new EventEmitter();
  errorMessage: string | null | undefined;
  file: FileModel;

  @Input() tasksConnectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _dialogRef: MatDialogRef<TaskFileUploadCancelComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: DeleteFilePageModel,
    private _fileService: FileService) {
      this.file = data.model;
      this.tasksConnectionContainer = data.tasksConnectionContainer;
  }

  ngOnInit(): void {
    this.tasksConnectionContainer.connection.on("DeletedFile", (_, fileId: number) =>
    {
      if (fileId == this.file.id)
        this._dialogRef.close();
    });
  }

  onSubmit()
  {
    this.loading = true;
    this.file.loadingParameters!.isStopped = true;
    this.cancellationStarted.emit();
      this._fileService.delete(this.tasksConnectionContainer.id, this.file.id).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
          this.cancelled.emit();
        },
        error: error => {
          this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          this.loading = false;
        }
      }
      );
  }
}
