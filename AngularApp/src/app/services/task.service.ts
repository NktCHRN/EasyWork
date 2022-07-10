import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { UserTaskModel } from '../shared/task/user-task.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';

@Injectable({
  providedIn: 'root'
})
export class TaskService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Tasks/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService) {
    super();
  }

  public get(token: string) : Observable<UserTaskModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<UserTaskModel[]>(this.serviceBaseURL, httpOptions)
      .pipe(map(array => {
        array.forEach(element => {
          element.startDate = new Date(element.startDate).toString();
          if (element.deadline)
            element.deadline = new Date(element.deadline).toString();
        })
        return array;
      }), catchError(this._processHTTPMsgService.handleError));
  }
}
