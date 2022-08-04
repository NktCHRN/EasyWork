import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { BannedModel } from '../shared/user/banned.model';
import { UserProfileReducedModel } from '../shared/user/user-profile-reduced.model';
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

  public getById(id: number) : Observable<UserModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.get<UserModel>(this.serviceBaseURL + id, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getActiveBansById(id: number) : Observable<BannedModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.get<BannedModel[]>(this.serviceBaseURL + id + '/activeBans', httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getFullName(firstName: string, lastName: string | null | undefined) : string {
    let fullName = firstName;
    if (lastName)
        fullName += ' ' + lastName;
    return fullName;
  }

  public get(search: string | null | undefined): Observable<UserProfileReducedModel[]>
  {
    let params = new HttpParams()
    if (search)
    {
      params = params.append('search', search);
    }
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      }),
      params: params
    };
    return this._http.get<UserProfileReducedModel[]>(this.serviceBaseURL, httpOptions)
    .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}
