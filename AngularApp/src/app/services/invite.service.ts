import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { UserOnProjectModel } from '../shared/project/user-on-project/user-on-project.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class InviteService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Invites/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService, private _tokenService: TokenService) {
    super();
  }

  public join(code: string) : Observable<UserOnProjectModel> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.post<UserOnProjectModel>(this.serviceBaseURL + code, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}
