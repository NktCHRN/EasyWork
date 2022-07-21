import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
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
}
