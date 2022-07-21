import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FileService } from 'src/app/services/file.service';
import { TokenService } from 'src/app/services/token.service';
import { FileModel } from 'src/app/shared/file/file.model';

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

  constructor(private _dialogRef: MatDialogRef<TaskFileDeleteComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: FileModel,
    private _fileService: FileService, private _tokenService: TokenService) {
      this.file = data;
  }

  ngOnInit(): void {
  }

  onSubmit()
  {
    this.loading = true;
      this._fileService.delete(this._tokenService.getJwtToken()!, this.file.id).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
        },
        error: error => {
          this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          this.loading = false;
        }
      }
      );
  }
}
