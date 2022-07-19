import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AddProjectModel } from '../shared/project/add-project.model';
import { InviteCodeStatusModel } from '../shared/project/invite/invite-code-status.model';
import { InviteCodeModel } from '../shared/project/invite/invite-code.model';
import { ProjectReducedModel } from '../shared/project/project-reduced.model';
import { ProjectModel } from '../shared/project/project.model';
import { UserOnProjectRole } from '../shared/project/user-on-project/role/user-on-project-role';
import { UpdateProjectModel } from '../shared/project/update-project.model';
import { UserOnProjectExtendedRawModel } from '../shared/project/user-on-project/user-on-project-extended-raw.model';
import { UserOnProjectExtendedModel } from '../shared/project/user-on-project/user-on-project-extended.model';
import { UserOnProjectReducedRawModel } from '../shared/project/user-on-project/user-on-project-reduced-raw.model';
import { UserOnProjectReducedModel } from '../shared/project/user-on-project/user-on-project-reduced.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { ProjectRoleService } from './project-role.service';
import { UpdateUserOnProjectModel } from '../shared/project/user-on-project/edit/update-user-on-project.model';
import { UserOnProjectQueryParametersModel } from '../shared/project/user-on-project/user-on-project-query-parameters.model';
import { AddUserOnProjectModel } from '../shared/project/user-on-project/add/add-user-on-project.model';
import { UserOnProjectModel } from '../shared/project/user-on-project/user-on-project.model';
import { ProjectLimitsModel } from '../shared/project/limits/project-limits.model';
import { TasksModel } from '../shared/project/tasks/tasks.model';
import { AddTaskModel } from '../shared/task/add-task.model';
import { TaskModel } from '../shared/task/task.model';
import { TaskReducedModel } from '../shared/task/task-reduced.model';
import { UserMiniWithAvatarModel } from '../shared/user/user-mini-with-avatar.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Projects/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService,
    private _roleService: ProjectRoleService) {
    super();
  }

  public get(token: string) : Observable<ProjectReducedModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<ProjectReducedModel[]>(this.serviceBaseURL, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public add(token: string, model: AddProjectModel) : Observable<ProjectModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.post<ProjectModel>(this.serviceBaseURL, model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getById(token: string, id: number) : Observable<ProjectModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<ProjectModel>(this.serviceBaseURL + id, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public update(token: string, id: number, model: UpdateProjectModel) : Observable<Object>
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

  public getReducedById(token: string, id: number) : Observable<ProjectReducedModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<ProjectReducedModel>(this.serviceBaseURL + id + "/reduced", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getMeAsProjectUser(token: string, id: number) : Observable<UserOnProjectReducedModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<UserOnProjectReducedRawModel>(this.serviceBaseURL + id + "/me", httpOptions)
      .pipe(map(response => {
        let result = new UserOnProjectReducedModel();
        result.role = UserOnProjectRole[response.role as keyof typeof UserOnProjectRole];
        result.userId = response.userId;
        return result;
      }), catchError(this._processHTTPMsgService.handleError));
  }

  public getInviteCode(token: string, id: number) : Observable<InviteCodeModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<InviteCodeModel>(this.serviceBaseURL + id + '/invite', httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public regenerateInviteCode(token: string, id: number) : Observable<string>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.post<string>(this.serviceBaseURL + id + '/invite', httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public changeInviteCodeStatus(token: string, id: number, model: InviteCodeStatusModel) : Observable<InviteCodeModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.put<InviteCodeModel>(this.serviceBaseURL + id + '/inviteStatus', model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getUsers(token: string, id: number, myRole: UserOnProjectRole): Observable<UserOnProjectExtendedModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<UserOnProjectExtendedRawModel[]>(this.serviceBaseURL + id + '/users', httpOptions)
      .pipe(map(response => {
        let result = response.map(u => {
          let result = u as unknown as UserOnProjectExtendedModel;
          result.role = UserOnProjectRole[u.role as keyof typeof UserOnProjectRole];
          result.isKickable = this._roleService.isKickable(myRole, result.role);
          return result;
        });
        return result;
      }), catchError(this._processHTTPMsgService.handleError));
  }

  public getUsersWithoutRoles(token: string, id: number): Observable<UserMiniWithAvatarModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<UserOnProjectExtendedRawModel[]>(this.serviceBaseURL + id + '/users', httpOptions)
      .pipe(map(response => {
        let result = response.map(u => {
          return u.user;
        });
        return result;
      }), catchError(this._processHTTPMsgService.handleError));
  }

  isSingleOwner(users: UserOnProjectExtendedModel[], me: UserOnProjectReducedModel) {
    return me.role == UserOnProjectRole.Owner && users.filter(u => u.role == UserOnProjectRole.Owner).length <= 1;
  }

  public leave(token: string, id: number) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.delete(this.serviceBaseURL + id + "/leave", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public kick(token: string, queryParams: UserOnProjectQueryParametersModel) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.delete(this.serviceBaseURL + queryParams.id + "/users/" + queryParams.userId, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public updateUser(token: string, queryParams: UserOnProjectQueryParametersModel, model: UpdateUserOnProjectModel) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.put(this.serviceBaseURL + queryParams.id + "/users/" + queryParams.userId, model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public addUser(token: string, id: number, model: AddUserOnProjectModel) : Observable<UserOnProjectModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.post<UserOnProjectModel>(this.serviceBaseURL + id + "/users/", model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getLimits(token: string, id: number) : Observable<ProjectLimitsModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<ProjectLimitsModel>(this.serviceBaseURL + id + "/limits", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public updateLimits(token: string, id: number, limits: ProjectLimitsModel) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.put(this.serviceBaseURL + id + "/limits", limits, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getTasks(token: string, id: number) : Observable<TasksModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<TasksModel>(this.serviceBaseURL + id + "/tasks/", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public addTask(token: string, id: number, model: AddTaskModel) : Observable<TaskModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.post<TaskModel>(this.serviceBaseURL + id + "/tasks/", model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public getArchivedTasks(token: string, id: number) : Observable<TaskReducedModel[]>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<TaskReducedModel[]>(this.serviceBaseURL + id + "/archive/", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}
