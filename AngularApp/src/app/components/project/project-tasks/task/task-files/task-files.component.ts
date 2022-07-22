import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { from, mergeMap, map, catchError, throwError, of, Observable, forkJoin, Subscription } from 'rxjs';
import { FileService } from 'src/app/services/file.service';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { FileWithTempId } from 'src/app/shared/file/file-with-temp-id.model';
import { FileModel } from 'src/app/shared/file/file.model';
import { BooleanContainer } from 'src/app/shared/other/booleancontainer';
import { TaskFileDeleteComponent } from './task-file-delete/task-file-delete.component';
import { TaskFileTooBigErrorComponent } from './task-file-too-big-error/task-file-too-big-error.component';
import { TaskFileUploadCancelComponent } from './task-file-upload-cancel/task-file-upload-cancel.component';

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

  isFormOpened: boolean = false;
  toAdd: FileWithTempId[] = [];
  showEmptyError: boolean = false;
  private readonly _maxFileSize: number = 1073741824;   // 1 GB
  cancelLockers: BooleanContainer[] = [];
  uploading: boolean = false;
  private _stop: EventEmitter<number> = new EventEmitter<number>();
  _fileCount: number = 0;
  _finishedCount: number = 0;

  constructor(private _tokenService: TokenService, private _taskService: TaskService, private _dialog: MatDialog,
  private _fileService: FileService, private _snackBar: MatSnackBar) {   }

  ngOnInit(): void {
    this._taskService.getFiles(this._tokenService.getJwtToken()!, this.taskId)
    .subscribe({
      next: result => this.files = result,
      error: error => {
        this.errorMessage = `$Files have not been loaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
      }
    });
  }

  toggleIsFormOpened(): void {
    this.isFormOpened = !this.isFormOpened;
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
          this.delete(file.id);
      }
    );
  }

  private delete(id: number): void {
    this.deletedFile.emit();
    const foundIndex = this.files.findIndex(e => e.id == id);
    if (foundIndex != -1)
      this.files.splice(foundIndex, 1);
  }

  onToAddChange(e: FileList): void {
    let maxId = 0;
    if (this.toAdd.length > 0)
      maxId = Math.max.apply(null, this.toAdd.map(f => f.tempId));
    this.toAdd = this.toAdd.concat(Array.from(e).map((item, index) => {
      return {
        tempId: maxId + index + 1,
        file: item,
        locked: false
      };
    }));
    const tooBigFiles: FileWithTempId[] = this.toAdd.filter(f => f.file.size > this._maxFileSize);
    this.toAdd = this.toAdd.filter(f => f.file.size <= this._maxFileSize);
    if (tooBigFiles.length > 0)
      this._dialog.open(TaskFileTooBigErrorComponent, {
        panelClass: "dialog-responsive",
        data: {
          fileNames: tooBigFiles.map(f => f.file.name),
          sizeLimit: this._maxFileSize
        }
      });
  }

  deleteToAdd(tempId: number): void {
    const found = this.toAdd.findIndex(f => f.tempId == tempId);
    if (found != -1)
      this.toAdd.splice(found, 1);
  }

  openCancelUploadDialog(file: FileModel): void
  {
    const dialogRef = this._dialog.open(TaskFileUploadCancelComponent, {
      panelClass: "dialog-responsive",
      data: file
    });
    dialogRef.componentInstance.cancellationStarted.subscribe(() => {
      this._stop.emit(file.id);
    });
    dialogRef.componentInstance.cancelled.subscribe(() => {
      this.delete(file.id);
    });
  }

  private finishUpload() {
    this._finishedCount++;
    if (this._fileCount == this._finishedCount)
    {
      this.uploading = false;
      this.isFormOpened = false;
      this._finishedCount = 0;
    }
  }

  onSubmit(): void {
    if (this.toAdd.length == 0)
    {
      this.showEmptyError = true;
      setTimeout(() => this.showEmptyError = false, 3000);
      return;
    }
    this.uploading = true;
    this.toAdd.forEach(f => f.locked = true);
    this._fileCount = this.toAdd.length;
    from(this.toAdd)
    .pipe(
      mergeMap(f => {
      return this._taskService.startFileUpload(this._tokenService.getJwtToken()!, this.taskId, {
        name: f.file.name
      })
      .pipe(map(result => {
        return {
          result: result,
          object: f
        }
      }), catchError((error: any) => 
        {
          this.finishUpload();
          const message = `File ${f.file.name} will not be uploaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          console.log(message);
          this._snackBar.open(message, "Close", {duration: 5000});
          this.deleteToAdd(f.tempId);
          return of();
        }));
    }))
    .subscribe({
      next: result => {
        this.deleteToAdd(result.object.tempId);
        const model: FileModel = result.result;
        model.size = result.object.file.size;
        model.loadingParameters = {
          isStopped: false,
          currentSize: 0
        }
        this.files.push(model);
        this.addedFile.emit();
        const chunkSize = 102400;
        let chunkIndex = 1;
        const chunkObservables: Observable<Object>[] = [];
        const chunkSubscriptions: Subscription[] = [];
        this._stop.subscribe(id => {
          if (model.id == id)
          {
            model.loadingParameters!.isStopped = true;
            chunkSubscriptions.forEach(o => o.unsubscribe());
          }
        });
        for(let offset = 0; offset < result.object.file.size; offset += chunkSize ){
          if (!model.loadingParameters.isStopped)
          {
            const chunk =  result.object.file.slice( offset, offset + chunkSize );
            const data = new FormData();
            data.append("chunk", chunk);
            const observable = this._fileService.sendChunk(this._tokenService.getJwtToken()!, result.result.id, chunkIndex, data);
            chunkObservables.push(observable);
            chunkSubscriptions.push(observable.subscribe(
              {
                next: () => 
                {
                  if (model.loadingParameters)
                    model.loadingParameters!.currentSize += chunk.size;
                },
                error: error => 
                {
                  if (!model.loadingParameters!.isStopped)
                  {
                    const message = `File ${model.name} will not be uploaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
                    console.log(message);
                    this._snackBar.open(message, "Close", {duration: 5000});
                    model.loadingParameters!.isStopped = true;
                  }
                }
              }));
            chunkIndex++;
          }
          else {
            chunkObservables.push(throwError(() => new Error("Upload stopped")));
          }
        }
        forkJoin(chunkObservables).subscribe({
          next: () => {
            this._fileService.endUpload(this._tokenService.getJwtToken()!, model.id)
            .subscribe({
              next: result => {
                model.isFull = result.isFull;
                model.loadingParameters = null;
                this.finishUpload();
              },
              error: error => {
                const message = `File ${model.name} will not be uploaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
                console.log(message);
                this._snackBar.open(message, "Close", {duration: 5000});
                model.loadingParameters = null;
                this.finishUpload();
              }
            });
          },
          error: () => {
            model.loadingParameters = null;
            console.log("An error occured");
            this.finishUpload();
          }
        });
      }
    });
  }
}