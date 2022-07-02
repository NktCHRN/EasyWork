import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError } from 'rxjs';
import { UserReducedModel } from '../shared/user-reduced.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';

@Injectable({
  providedIn: 'root'
})
export class UserinfoService extends BaseService {

  constructor(private http: HttpClient,
    private processHTTPMsgService: ProcessHTTPMsgService) {
    super();
  }

  override serviceBaseURL: string = this.baseURL + 'UserInfo/';

  private static lastUser : UserReducedModel | null | undefined;

  public getLastUser() : UserReducedModel | null | undefined
  {
    return UserinfoService.lastUser;
  }

  public setLastUser(user : UserReducedModel | null | undefined)
  {
    UserinfoService.lastUser = user;
  }

  public get(token: string): Observable<UserReducedModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this.http.get<UserReducedModel>(this.serviceBaseURL, httpOptions)
      .pipe(catchError(this.processHTTPMsgService.handleError));
  }
}
