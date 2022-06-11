import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError } from 'rxjs';
import { UserReducedModule } from '../shared/UserReducedModel';
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

  public get(token: string): Observable<UserReducedModule>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this.http.get<UserReducedModule>(this.serviceBaseURL, httpOptions)
      .pipe(catchError(this.processHTTPMsgService.handleError));
  }
}
