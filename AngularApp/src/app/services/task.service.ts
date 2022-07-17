import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AddMessageModel } from '../shared/message/add-message.model';
import { MessageModel } from '../shared/message/message.model';
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

  public update(token: string, id: number, model: UpdateTaskModel) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.put(this.serviceBaseURL + id, model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public delete(token: string, id: number) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.delete(this.serviceBaseURL + id, httpOptions)
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

  public getMessages(token: string, id: number) : Observable<MessageModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<MessageModel[]>(this.serviceBaseURL + id + '/messages', httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public addMessage(token: string, id: number, model: AddMessageModel) : Observable<MessageModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
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
