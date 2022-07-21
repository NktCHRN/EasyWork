import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { FileService } from 'src/app/services/file.service';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { FileModel } from 'src/app/shared/file/file.model';
import { TaskFileDeleteComponent } from './task-file-delete/task-file-delete.component';

@Component({
  selector: 'app-task-files',
  templateUrl: './task-files.component.html',
  styleUrls: ['./task-files.component.scss']
})
export class TaskFilesComponent implements OnInit {
  @Input() taskId: number = undefined!;
  editable: boolean = false;
  files: FileModel[] = undefined!;
  errorMessage: string | null | undefined;
  @Output() deletedFile = new EventEmitter();
  @Output() addedFile = new EventEmitter();

  constructor(private _tokenService: TokenService, private _taskService: TaskService, private _dialog: MatDialog,
  private _fileService: FileService) {   }

  ngOnInit(): void {
    this._taskService.getFiles(this._tokenService.getJwtToken()!, this.taskId)
    .subscribe({
      next: result => this.files = result,
      error: error => {
        this.errorMessage = `$Files have not been loaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
      }
    });
  }

  toggleEditable(): void {
    this.editable = !this.editable;
  }

  download(id: number): void {
    this._fileService.download(this._tokenService.getJwtToken()!, id)
    .subscribe({
      next: result => {
        const fileName = result.headers.get('content-disposition')
        ?.split(';')[1].split('=')[1];
        const blob: Blob = result.body as Blob;
        const a = document.createElement('a');
        a.download = fileName ?? '';
        a.href = window.URL.createObjectURL(blob);
        a.click();
        a.remove();
      }
    });
  }

  openDeleteDialog(file: FileModel) {
    const dialogRef = this._dialog.open(TaskFileDeleteComponent, {
      panelClass: "dialog-responsive",
      data: file
    });
    dialogRef.afterClosed().subscribe(
      () => {
        if (dialogRef.componentInstance.success)
        {
          this.deletedFile.emit();
          const foundIndex = this.files.findIndex(e => e.id == file.id);
          if (foundIndex != -1)
            this.files.splice(foundIndex, 1);
        }
      }
    );
  }
}
