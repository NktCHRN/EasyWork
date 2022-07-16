import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { TaskPriority } from '../shared/task/priority/task-priority';
import { TaskModel } from '../shared/task/task.model';
import { UserTaskModel } from '../shared/task/user-task.model';
import { UserMiniWithAvatarModel } from '../shared/user/user-mini-with-avatar.model';
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

  public getById(token: string, id: number) : Observable<TaskModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<TaskModel>(this.serviceBaseURL + id, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }


  public getExecutors(token: string, id: number) : Observable<UserMiniWithAvatarModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<UserMiniWithAvatarModel[]>(this.serviceBaseURL + id + '/users', httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getPrioritiesWithColors()
  {
    return {
      [TaskPriority.Lowest] : "mediumseagreen",
      [TaskPriority.Low] : "green",
      [TaskPriority.Middle] : "orange",
      [TaskPriority.High] : "#D84315",
      [TaskPriority.Highest] : "red"
    };
  }
}
