import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FileService } from 'src/app/services/file.service';
import { TokenService } from 'src/app/services/token.service';
import { FileModel } from 'src/app/shared/file/file.model';
import { EventEmitter } from '@angular/core';

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

  constructor(private _dialogRef: MatDialogRef<TaskFileUploadCancelComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: FileModel,
    private _fileService: FileService, private _tokenService: TokenService) {
      this.file = data;
  }

  ngOnInit(): void {
  }

  onSubmit()
  {
    this.loading = true;
    this.file.loadingParameters!.isStopped = true;
    this.cancellationStarted.emit();
      this._fileService.delete(this._tokenService.getJwtToken()!, this.file.id).subscribe(
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
