import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { UserOnProjectModel } from '../shared/project/user-on-project/user-on-project.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';

@Injectable({
  providedIn: 'root'
})
export class InvitesService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Invites/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService) {
    super();
  }

  public join(token: string, code: string) : Observable<UserOnProjectModel> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.post<UserOnProjectModel>(this.serviceBaseURL + code, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}
