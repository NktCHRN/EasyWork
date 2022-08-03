import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { baseURL } from '../shared/constants/baseurl';
import { GanttModel } from '../shared/project/gantt/gantt.model';
import { Month } from '../shared/project/gantt/month';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class GanttService {

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService,
    private _tokenService: TokenService) { }

  public getMonths(from: Date, to: Date): Month[]
  {
    let result: Month[] = [];
    let current = new Date(from);
    do
    {
      const month: Month = {
        month: new Date(current),
        days: []
      };
      result.push(month);
      do
      {
        month.days.push({
          number: current.getDate(),
          dayOfWeek: current.getDay()
        });
        current.setDate(current.getDate() + 1);
      } while(current.getDate() != 1 && current <= to)
    } while(current <= to);
    return result;
  }

  public get(projectId: number, from: Date, to: Date) : Observable<GanttModel>
  {
    const params = new HttpParams().appendAll({
      'from': from.toISOString(),
      'to': to.toISOString()
    });
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      }),
      params: params
    };
    return this._http.get<GanttModel>(baseURL + "projects/" + projectId + "/gantt/", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}
