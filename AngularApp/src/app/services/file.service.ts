import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, retry } from 'rxjs';
import { FileModel } from '../shared/file/file.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';

@Injectable({
  providedIn: 'root'
})
export class FileService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Files/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService) {
    super();
  }

  public download(token: string, id: number)
  {
    return this._http.get(this.serviceBaseURL + id,{
      observe: 'response',
      responseType: 'blob',
      headers: new HttpHeaders({
        'Authorization': 'Bearer ' + token
      })
    })
    .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public delete(token: string, id: number) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.delete(this.serviceBaseURL + id, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public sendChunk(token: string, id: number, index: number, chunk: FormData): Observable<Object> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.post(this.serviceBaseURL + id + "/chunk/" + index, chunk, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public endUpload(token: string, id: number): Observable<FileModel> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
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
