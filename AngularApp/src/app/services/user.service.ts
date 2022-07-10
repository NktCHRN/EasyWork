import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { UserModel } from '../shared/user/user.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';

@Injectable({
  providedIn: 'root'
})
export class UserService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Users/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService) {
    super();
  }

  public get(id: number) : Observable<UserModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.get<UserModel>(this.serviceBaseURL + id, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getFullName(firstName: string, lastName: string | null | undefined) : string {
    let fullName = firstName;
    if (lastName)
        fullName += ' ' + lastName;
    return fullName;
  }
}
