import { Component, EventEmitter, Inject, Input, OnInit, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FileService } from 'src/app/services/file.service';
import { DeleteFilePageModel } from 'src/app/shared/file/delete-file-page.model';
import { FileModel } from 'src/app/shared/file/file.model';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';

@Component({
  selector: 'app-task-file-delete',
  templateUrl: './task-file-delete.component.html',
  styleUrls: ['./task-file-delete.component.scss']
})
export class TaskFileDeleteComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  file: FileModel;
  @Output() succeeded = new EventEmitter();

  @Input() tasksConnectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _dialogRef: MatDialogRef<TaskFileDeleteComponent>, 
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
      this._fileService.delete(this.tasksConnectionContainer.id, this.file.id).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
          this.succeeded.emit();
        },
        error: error => {
          this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          this.loading = false;
        }
      }
      );
  }
}
