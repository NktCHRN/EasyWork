import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AddProjectModel } from '../shared/project/add-project.model';
import { ProjectReducedModel } from '../shared/project/project-reduced.model';
import { ProjectModel } from '../shared/project/project.model';
import { UserOnProjectRole } from '../shared/project/role/user-on-project-role';
import { UserOnProjectReducedModel } from '../shared/project/user-on-project/user-on-project-reduced.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';

@Injectable({
  providedIn: 'root'
})
export class ProjectsService extends BaseService {
  override serviceBaseURL: string = this.baseURL + 'Projects/';

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService) {
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
    return this._http.get<UserOnProjectReducedModel>(this.serviceBaseURL + id + "/me", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}
