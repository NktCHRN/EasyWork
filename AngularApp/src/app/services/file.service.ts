import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, retry } from 'rxjs';
import { FileModel } from '../shared/file/file.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class FileService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Files/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService, private _tokenService: TokenService) {
    super();
  }

  public download(id: number)
  {
    return this._http.get(this.serviceBaseURL + id,{
      observe: 'response',
      responseType: 'blob',
      headers: new HttpHeaders({
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    })
    .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public delete(tasksConnectionId: string | null, id: number) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue(),
        'TasksConnectionId': tasksConnectionId ?? ''
      })
    };
    return this._http.delete(this.serviceBaseURL + id, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public sendChunk(id: number, index: number, chunk: FormData): Observable<Object> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.post(this.serviceBaseURL + id + "/chunk/" + index, chunk, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public endUpload(connectionId: string | null, id: number): Observable<FileModel> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue(),
        'ConnectionId': connectionId ?? ''
      })
    };
    return this._http.post<FileModel>(this.serviceBaseURL + id + "/end", httpOptions)
      .pipe(retry({
        count: 3,
        delay: 1000
      }))
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}
