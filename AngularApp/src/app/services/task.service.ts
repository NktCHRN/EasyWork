import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AddFileModel } from '../shared/file/add-file.model';
import { FileModel } from '../shared/file/file.model';
import { AddMessageModel } from '../shared/message/add-message.model';
import { MessageModel } from '../shared/message/message.model';
import { AddExecutorModel } from '../shared/task/executor/add-executor.model';
import { TaskPriority } from '../shared/task/priority/task-priority';
import { TaskPriorityNone } from '../shared/task/priority/task-priority-none';
import { TaskStatus } from '../shared/task/status/task-status';
import { TaskStatusWithDescriptionModel } from '../shared/task/status/task-status-with-description.model';
import { TaskReducedModel } from '../shared/task/task-reduced.model';
import { TaskModel } from '../shared/task/task.model';
import { UpdateTaskModel } from '../shared/task/update-task.model';
import { UserTaskModel } from '../shared/task/user-task.model';
import { UserMiniWithAvatarModel } from '../shared/user/user-mini-with-avatar.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class TaskService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Tasks/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService,
    private _tokenService: TokenService) {
    super();
  }

  public get() : Observable<UserTaskModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
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

  public getById(id: number) : Observable<TaskModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.get<TaskModel>(this.serviceBaseURL + id, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public update(id: number, model: UpdateTaskModel) : Observable<Object>
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

  public deleteExecutor(id: number, userId: number) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.delete(this.serviceBaseURL + id + "/users/" + userId, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public addExecutor(id: number, model: AddExecutorModel) : Observable<UserMiniWithAvatarModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.post<UserMiniWithAvatarModel>(this.serviceBaseURL + id + '/users', model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getExecutors(id: number) : Observable<UserMiniWithAvatarModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.get<UserMiniWithAvatarModel[]>(this.serviceBaseURL + id + '/users', httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getMessages(id: number) : Observable<MessageModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.get<MessageModel[]>(this.serviceBaseURL + id + '/messages', httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getFiles(id: number) : Observable<FileModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.get<FileModel[]>(this.serviceBaseURL + id + '/files', httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public startFileUpload(id: number, model: AddFileModel) : Observable<FileModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.post<FileModel>(this.serviceBaseURL + id + '/files/start', model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }


  public addMessage(id: number, model: AddMessageModel) : Observable<MessageModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.post<MessageModel>(this.serviceBaseURL + id + '/messages', model, httpOptions)
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

  public getStatusesWithDescriptions(addArchived: boolean) : TaskStatusWithDescriptionModel[] {
    const statuses:TaskStatusWithDescriptionModel[] = [
      {
        status: TaskStatus.ToDo,
        description: "To Do"
      },
      {
        status: TaskStatus.InProgress,
        description: "In Progress"
      },
      {
        status: TaskStatus.Validate,
        description: "Validate"
      },
      {
        status: TaskStatus.Complete,
        description: "Done"
      },
    ];
    if (addArchived)
      statuses.push({
        status: TaskStatus.Archived,
        description: "Archived"
      });
    return statuses;
  }

  public getSortedPriorities() : (TaskPriority | TaskPriorityNone)[] {
    const priorities: (TaskPriority | TaskPriorityNone)[] = [
      TaskPriorityNone.None,
      TaskPriority.Lowest,
      TaskPriority.Low,
      TaskPriority.Middle,
      TaskPriority.High,
      TaskPriority.Highest
    ];
    return priorities;
  }

  public ExtendedPriorityToPriorityOrNull(priority: TaskPriority | TaskPriorityNone) : TaskPriority | null | undefined {
    return (priority in TaskPriority) ? <TaskPriority>priority : null;
  }

  public PriorityOrNullToExtendedPriority(priority: TaskPriority | null | undefined) : TaskPriority | TaskPriorityNone {
    return priority ? priority : TaskPriorityNone.None;
  }

  getInsertAtIndexByTaskId(taskId: number, tasks: TaskReducedModel[]): number {
    let i = 0;
    while (i < tasks.length && tasks[i].id < taskId)
      ++i;
    return i;
  }
}
