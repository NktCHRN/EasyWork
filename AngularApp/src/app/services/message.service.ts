import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { AddMessageModel } from '../shared/message/add-message.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Messages/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService, private _tokenService: TokenService) {
    super();
  }

  public delete(id: number) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.delete(this.serviceBaseURL + id, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public update(id: number, model: AddMessageModel) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.put(this.serviceBaseURL + id, model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

}
