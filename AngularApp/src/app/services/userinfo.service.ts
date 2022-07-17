import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, BehaviorSubject } from 'rxjs';
import { UserReducedModel } from '../shared/user/user-reduced.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';

@Injectable({
  providedIn: 'root'
})
export class UserInfoService extends BaseService {

  constructor(private http: HttpClient,
    private processHTTPMsgService: ProcessHTTPMsgService) {
    super();
  }

  override serviceBaseURL: string = this.baseURL + 'UserInfo/';

  private _lastUser: BehaviorSubject<UserReducedModel | null | undefined> = new BehaviorSubject<UserReducedModel | null | undefined>(undefined);
  public lastUser: Observable<UserReducedModel | null | undefined> = this._lastUser.asObservable();

  public setLastUser(user : UserReducedModel | null | undefined)
  {
    if (user?.avatarURL)
      user.avatarURL += "?" + Date.now().toString();
    this._lastUser.next(user);
  }

  public updateLastUser() : void {
    this.get(localStorage.getItem('jwt')!)
    .subscribe(user => this.setLastUser(user));
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
